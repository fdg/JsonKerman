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
	 * Crew list jQuery widget.
	 */
	$.widget('krimzon.crewlist', {

		capcity: 0,

		options: {
		},

		_create: function() {
			this.element.html('');
		},

		setCapacity: function(capacity) {
			this.capacity = capacity;
		},

		setData: function(crewData) {
			var filled = 0;
			this.element.html('');
			for (var crewMember in crewData) {
				// TODO: We can also do bars for courage and stupidity.
				this.element.append($('<li>' + crewMember + '</li>'));
				filled += 1;
			}
			for (var i = filled; i < this.capacity; i += 1) {
				this.element.append($('<li></li>'));
			}
		}

	});

})(jQuery);
