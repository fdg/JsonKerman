/*
Copyright (c) 2014 David Laurie

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

"use strict";

/**
 * A layer over the top of Advanced Javascript And Kerbal Stuff to provide a
 * more convenient interface for game data.
 *
 * @todo: Detect more events: gameChange, activeVesselChange, currentBodyChange.
 */
function Kerbal() {
	var self = this,
		ajaks = new AJAKS(),
		callbacks = {},
		currentScene = '',
		activeVesselId = '',
		activeVesselBodyIndex = -1;

	/**
	 * Expose the raw data anyway.
	 */
	this.data = {};

	/**
	 * Returns the nice name for the current game scene.
	 */
	this.getCurrentGameSceneName = function() {
		if (!this.data.hasOwnProperty('currentScene')) {
			return '';
		}
		// TODO: This list is incomplete.
		switch (this.data['currentScene']) {
			case 'kspsplashscreen': return 'Loading';
			case 'kspMainMenu':     return 'Main Menu';
			case 'kspsettings':     return 'Settings';
			case 'spaceCenter':     return 'Space Center';
			case 'editor':          return 'Vehicle Assembly Building';
			case 'planeEditor':     return 'Spaceplane Hangar';
			case 'trackingStation': return 'Tracking Station';
			case 'pFlight2':        return 'In Flight';
		}
		return 'Unknown';
	};

	/**
	 * Returns true if we're playing a game (not in the menus or splash).
	 */
	this.isInGame = function() {
		switch (this.data['currentScene']) {
			case 'kspsplashscreen':
			case 'kspMainMenu':
			case 'kspsettings':
				return false;
		}
		return true;
	};

	/**
	 * Returns true if we're in flight.
	 */
	this.isInFlight = function() {
		return this.data.isFlight;
	};

	/**
	 * Returns a value nested within data, or a default value if no such node
	 * was found. e.g. tryGetValue('flightGlobals.activeVessel.altitude', 0);
	 */
	this.tryGetValue = function(path, defaultValue) {
		if (typeof defaultValue === 'undefined') {
			defaultValue = '';
		}
		var parts = path.split('.');
		var currentNode = this.data;
		while (parts.length > 0) {
			if (!currentNode.hasOwnProperty(parts[0])) {
				return defaultValue;
			}
			currentNode = currentNode[parts.shift()];
		}
		return currentNode;
	};

	/**
	 * Format a number. Should go in a utility class instead maybe.
	 */
	this.formatNumber = function(num) {
		return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
	};

	/**
	 * Format a time in format [1y [1d [11h [11m]]] 11s
	 */
	this.formatTime = function(time) {
		var parts = this.timeToParts(time);
		
		var timeStr = parts.seconds + 's';
		if (parts.minutes || parts.hours || parts.days || parts.years) {
			timeStr = parts.minutes + 'm ' + timeStr;
		}
		if (parts.hours || parts.days || parts.years) {
			timeStr = parts.hours + 'h ' + timeStr;
		}
		if (parts.days || parts.years) {
			timeStr = parts.days + 'd ' + timeStr;
		}
		if (parts.years) {
			timeStr = parts.years + 'y';
		}
		return timeStr;
	};

	/**
	 * Format a date in format Year 1 Day 1 11h 11m
	 */
	this.formatDate = function(date) {
		var parts = this.timeToParts(date);

		parts.years += 1;
		parts.days += 1;
		if (parts.hours < 10) {
			parts.hours = "0" + parts.hours;
		}
		if (parts.minutes < 10) {
			parts.minutes = "0" + parts.minutes;
		}

		return "Year " + parts.years + ", Day " + parts.days + ", " + parts.hours + ":" + parts.minutes;
	};

	/**
	 * Convert a time into time parts. { years, days, hours, minutes, seconds }
	 */
	this.timeToParts = function(time) {
		var parts = {};

		time = Math.round(time);

		parts.seconds = time % 60;
		time -= parts.seconds;
		time /= 60;

		parts.minutes = time % 60;
		time -= parts.minutes;
		time /= 60;
		
		parts.hours = time % 6;
		time -= parts.hours;
		time /= 6;

		parts.days = time % 411;
		time -= parts.days;
		time /= 426;

		parts.years = time;

		return parts;
	};

	/**
	 * Private method to call a callback with an event.
	 */
	function callCallback(event, parm) {
		if (!callbacks.hasOwnProperty(event)) {
			return;
		}
		for (var ns in callbacks[event]) {
			callbacks[event][ns](parm);
		}
	}

	/**
	 * Adds a callback for a given event. The event can be prefixed with a
	 * namespace, e.g. "namespace.eventname" to allow specifically this
	 * callback to be unregistered again.
 	 */
	this.on = function(event, callback) {
		var parts = event.split('.'),
			ns = parts[0],
			ev = (parts.length > 1) ? parts[1] : parts[0];

		if (!callbacks.hasOwnProperty(ev)) {
			callbacks[ev] = {};
		}
		callbacks[ev][ns] = callback;
	};

	/**
	 * Unregisters the named event.
	 */
	this.off = function(event) {
		var parts = event.split('.'),
			ns = parts[0],
			ev = (parts.length > 1) ? parts[1] : parts[0];

		if (callbacks.hasOwnProperty(ev)) {
			delete callbacks[ev][ns];
		}
	};

	function onDataUpdate() {
		self.data = ajaks.data;

		var newScene = self.tryGetValue('currentScene', '');
		if (newScene != currentScene) {
			currentScene = newScene;
			callCallback('sceneChange', newScene);
		}

		var newVesselId = self.tryGetValue('flightGlobals.activeVessel.id', '');
		if (newVesselId != activeVesselId) {
			activeVesselId = newVesselId;
			callCallback('activeVesselChange', self.tryGetValue('flightGlobals.activeVessel', {}));
		}

		var newBody = self.tryGetValue('flightGlobals.activeVessel.celestialBody.flightsGlobalIndex', -1);
		if (newBody != activeVesselBodyIndex) {
			activeVesselBodyIndex = newBody;
			callCallback('currentBodyChange', self.tryGetValue('flightGlobals.activeVessel.celestialBody', {}));
		}

		callCallback('dataUpdate');
	}

	function onServiceUp() {
		callCallback('serviceUp');
		callCallback('serviceUpdate', true);
	}

	function onServiceDown() {
		callCallback('serviceDown');
		callCallback('serviceUpdate', false);
	}

	function onConnectionUp() {
		callCallback('connectionUp');
		callCallback('connectionUpdate', true);
	}

	function onConnectionDown() {
		callCallback('connectionDown');
		callCallback('connectionUpdate', false);
	}

	this.start = function() {
		ajaks.start({
			dataUpdate: onDataUpdate,
			serviceUp: onServiceUp,
			serviceDown: onServiceDown,
			connectionUp: onConnectionUp,
			connectionDown: onConnectionDown
		});
	};
}

var kerbal = new Kerbal();

