//
//  Copyright (c) 2015 IBM Corp. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
//  except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software distributed under the
//  License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
//  either express or implied. See the License for the specific language governing permissions 
//  and limitations under the License.
using System;

namespace IBM.Cloudant.Client
{
	/// <summary>
	/// Search field.
	/// </summary>
	public struct TextIndexField
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public String name { get; set; }

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		public TextIndexFieldType type { get; set; }
	}

	/// <summary>
	/// Search field type.
	/// </summary>
	public enum TextIndexFieldType
	{
		/// <summary>
		/// The number.
		/// </summary>
		Number,
		/// <summary>
		/// The boolean.
		/// </summary>
		Boolean,
		/// <summary>
		/// The string.
		/// </summary>
		String
	}
}

