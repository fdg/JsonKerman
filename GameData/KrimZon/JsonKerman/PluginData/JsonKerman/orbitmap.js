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

(function($){
	/**
	 * Orbit map jQuery widget.
	 * +latitude = north from equator, +longitude = east from meridian
	 */
	$.widget('krimzon.orbitmap', {

		width: 1,

		height: 1,

		spacecraft: {
		},

		options: {
		},

		_create: function() {
			var self = this;

			this.element.css({
				'width': '100%',
				'position': 'relative'
			});

			// Make element auto-resize
			function heightForWidth() {
				self._updateLabels();
			}
			$(window).resize(heightForWidth);

			// Add labels container
			this.labelview = $('<div></div>');
			this.element.append(this.labelview);

			// Add canvas
			//this.canvas = $('<canvas style="width: 100%; height: 100%;"></canvas>');
			//this.element.append(this.canvas);
		},

		_adjustHeight: function() {
			this.width = this.element.width();
			this.height = Math.floor(this.width * 0.5);
			this.element.css('height', this.height + 'px');
		},

		//_drawCanvas: function() {
		//	context = this.canvas[0].getContext();
		//	this.canvas.width(this.width).height(this.height);
		//	context.fillStyle = "rgba(1, 1, 1, 0)";
		//	context.fillRect(0, 0, this.width, this.height);
		//	// TODO: Draw stuff
		//},

		_updateLabels: function() {
			this._adjustHeight();
			this.labelview.html(''); 
			for (var craft in this.spacecraft) {
				var latitude = this.spacecraft[craft].lat,
					longitude = this.spacecraft[craft].lng,
					coords = this._geoToPixels(latitude, longitude),
					pin = $('<div class="pin"></div>'),
					label = $('<div class="label">' + craft + '</div>');
				pin.css({
					'width': '3px',
					'height': '3px',
					'position': 'absolute',
					'left': (coords[0] - 1) + 'px',
					'top': (coords[1] - 1) + 'px',
					'z-index': '2'
				});
				label.css({
					'position': 'absolute',
					'left': coords[0] + 'px',
					'top': coords[1] + 'px',
					'z-index': '2'
				});

				if (this.spacecraft[craft].hasOwnProperty('cssClass')) {
					pin.addClass(this.spacecraft[craft].cssClass);
					label.addClass(this.spacecraft[craft].cssClass);
				}

				this.labelview.append(pin);
				this.labelview.append(label);
			}
		},

		_geoToPixels: function(lat, lng) {
			while (lng < -180) { lng += 360; }
			while (lng > 180) { lng -= 360; }
			while (lat < -180) { lat += 360; }
			while (lat > 180) { lat -= 360; }
			return [
				// lng of -180 to +180 becomes x of 0 to W
				(lng + 180) * (this.width / 360.0),
				// lat of -90 to +90 becomes y of H to 0
				this.height - ((lat + 90) * (this.height / 180.0))
			];
		},

		/**
		 * Updates a craft. We either specify a name and an array of lat/long
		 * to add it, or we just specify the name to remove it.
		 *
		 * Data should be an object with required keys (lat, lng) and
		 * optional keys (cssClass).
		 */
		updateCraft: function(craft, data) {
			if (typeof data === 'undefined') {
				delete this.spacecraft[craft];
				return;
			}
			this.spacecraft[craft] = data;
			this._updateLabels();
		},

		/**
		 * Clears all craft.
		 */
		clearAllCraft: function() {
			this.spacecraft = {};
		},

		/**
		 * Sets the map image.
		 * The image needs to have an aspect ratio of 2:1.
		 */
		setMapImage: function(image) {
			this.element.css({
				'background-image': 'url(' + image + ')',
				'background-size': 'cover'
			});
		}

	});

})(jQuery);
