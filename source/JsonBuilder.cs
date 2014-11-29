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
using System;
using System.Text;
using UnityEngine;

namespace JsonKerman
{
	/**
	 * Quick and dirty immediate-mode json builder.
	 */
	public class JsonBuilder
	{
		private StringBuilder builder;
		private int objectLevel;
		bool firstPair;

		/**
		 * Creates a new json builder with an object started.
		 * The object will automatically be ended when ToString is called.
		 */
		public JsonBuilder()
		{
			builder = new StringBuilder();
			builder.Append("{");
			objectLevel = 1;
			firstPair = true;
		}

		/**
		 * Adds a value to the current object with the given key.
		 */
		public void AddValue(string k, object v)
		{
			if (!firstPair)
			{
				builder.Append(",");
			}
			firstPair = false;
			builder.Append("\n");
			builder.Append(new string('\t', objectLevel));

			builder.Append(CleanString(k));
			builder.Append(": ");
			if (v is bool)
			{
				builder.Append(BoolToString((bool)v));
			}
			else if (v is string)
			{
				builder.Append(CleanString((string)v));
			}
			else if (v is float)
			{
				builder.Append(((float)v).ToString());
			}
			else if (v is double)
			{
				builder.Append(((double)v).ToString());
			}
			else if (v is int)
			{
				builder.Append(((int)v).ToString());
			}
			else if (v is Vector3d)
			{
				Vector3d vec = (Vector3d)v;
				builder.Append("[");
				builder.Append(vec.x.ToString());
				builder.Append(", ");
				builder.Append(vec.y.ToString());
				builder.Append(", ");
				builder.Append(vec.z.ToString());
				builder.Append("]");
			}
			else if (v is Vector3)
			{
				Vector3 vec = (Vector3)v;
				builder.Append("[");
				builder.Append(vec.x.ToString());
				builder.Append(", ");
				builder.Append(vec.y.ToString());
				builder.Append(", ");
				builder.Append(vec.z.ToString());
				builder.Append("]");
			}
			else
			{
				builder.Append("null");
			}
		}

		public void StartObject(string k)
		{
			if (!firstPair)
			{
				builder.Append(",");
			}
			firstPair = false;
			builder.Append("\n");
			builder.Append(new string('\t', objectLevel));

			builder.Append(CleanString(k));
			builder.Append(": {");
			objectLevel += 1;
			firstPair = true;
		}

		public void EndObject()
		{
			if (objectLevel < 1)
			{
				throw new InvalidOperationException("Too many EndObject calls (mismatched for the number of prior StartObject calls at this point)");
			}

			objectLevel -= 1;
			firstPair = false;
			builder.Append("\n");
			builder.Append(new string('\t', objectLevel));
			builder.Append("}");
		}

		/**
		 * Converts the json builder to a string.
		 * The builder should not be used after that.
		 */
		public override string ToString()
		{
			builder.Append("\n}");
			return builder.ToString();
		}

		private string CleanString(string s)
		{
			return "\"" + s
				.Replace("\\", "\\\\")
				.Replace("\"", "\\\"")
				.Replace("/", "\\/")
				.Replace("\b", "\\b")
				.Replace("\n", "\\n")
				.Replace("\r", "\\r")
				.Replace("\t", "\\t") + "\"";
		}

		private string BoolToString(bool b)
		{
			return b ? "true" : "false";
		}
	}
}

