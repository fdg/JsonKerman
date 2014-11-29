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
 * Advanced Javascript And Kerbal Stuff.
 *
 * Fairly simple module to regularly retrieve data and fire off custom jquery
 * events about it. Requires jQuery, but that dependency could probably be
 * removed.
 *
 * This will eventually sit behind a nicer API.
 */
function AJAKS() {
	var apiUrl,
		requestStartTime, // Last time we started an ajax request.
		lastSuccessTime, // Last time the service returned successfully.
		_callbacks = {
			dataUpdate: function(){},
			connectionUp: function(){},
			connectionDown: function(){},
			serviceUp: function(){},
			serviceDown: function(){}
		};

	/**
	 * Time in milliseconds between heartbeats.
	 */
	this.heartbeatTime = 2000;

	/**
	 * Timeout for individual requests in milliseconds.
	 * This is the time after which we give up with the current request and try
	 * again.
	 */
	this.requestTimeout = 5000;

	/**
	 * Timeout for the overall service.
	 * This is the time after which the service is considered to be offline,
	 * e.g. when you stopped playing.
	 */
	this.serviceTimeout = 20000;

	/**
	 * Stores the latest data retrieved from the API.
	 */
	this.data = {};

	/**
	 * Connection status.
	 */
	this.status = {
		connection: false,
		service: false
	};

	/**
	 * Connect to the API.
	 * The default URL to connect to is 'jebservice.ksp'.
	 * Note: ksp here stands for Kerbal Server Pages.
	 */
	this.start = function(callbacks, url) {
		if (typeof url === 'undefined') {
			url = 'jebservice.ksp';
		}
		apiUrl = url;

		if (typeof callbacks === 'object') {
			for (var key in _callbacks) {
				if (callbacks.hasOwnProperty(key)) {
					_callbacks[key] = callbacks[key];
				}
			}
		}

		this._makeAjaxRequest();
	};

	this._makeAjaxRequest = function() {
		requestStartTime = new Date().getTime();
		var self = this;
		$.ajax({
			url: apiUrl,
			timeout: this.requestTimeout,
			cache: false,
			dataType: 'json' // this will make _ajaxSuccess data parameter be a json object.
		})
			.done(function(d, t, x){ self._ajaxSuccess(d, t, x); })
			.fail(function(x, t, e){ self._ajaxError(x, t, e); })
			.always(function(x, t){ self._ajaxComplete(x, t); });
	};

	this._ajaxSuccess = function(data, textStatus, xhr) {
		lastSuccessTime = new Date().getTime();

		if (!this.status.service) {
			_callbacks.serviceUp();
			this.status.service = true;
		}
		if (!this.status.connection) {
			_callbacks.connectionUp();
			this.status.connection = true;
		}

		this.data = data;
		_callbacks.dataUpdate();
	};

	this._ajaxError = function(xhr, textStatus, errorThrown) {
		// textStatus is null, "timeout", "error", "abort", or "parsererror"
		if (this.status.connection) {
			_callbacks.connectionDown();
			this.status.connection = false;
		}

		var downTime = new Date().getTime() - lastSuccessTime;
		if (downTime > this.serviceTimeout) {
			if (this.status.service) {
				_callbacks.serviceDown();
				this.status.service = false;
			}
		}
	};

	this._ajaxComplete = function(xhr, textStatus) {
		var requestTimeSoFar = new Date().getTime() - requestStartTime;
		if (requestTimeSoFar < this.heartbeatTime) {
			var self = this;
			setTimeout(function(){ self._makeAjaxRequest(); }, this.heartbeatTime - requestTimeSoFar);
		} else {
			this._makeAjaxRequest();
		}
	};
};

