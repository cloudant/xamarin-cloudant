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
	/// IndexField is a class for creating an index field with options (e.g. sort order) for querying.
	/// </summary>
	/// <remarks>
	/// This class is used for creating an index field with options.
	/// </remarks>
	public class IndexField
	{
		/// <summary>
		/// SortOrder is an enum that specifies the sort order.
		/// </summary>
		/// <remarks>
		/// This enumerates the sort order values.
		/// </remarks>
		public enum SortOrder{
			/// <summary>
			/// Ascending sort order
			/// </summary>
			asc,

			/// <summary>
			/// Descending sort order
			/// </summary>
			desc
		}

		/// <summary>
		/// Name of the index field.
		/// </summary>
		public string name { private set; get; }

		/// <summary>
		/// Sort order of this index field.
		/// </summary>
		public SortOrder sortOrder { private set; get;}

		/// <summary>
		/// Represents a Cloudant Sort Syntax for a json field. Used to specify
		/// an element of the 'index.fields' array (POST db/_index) and 'sort' array (db/_find).
		/// Sort syntax documentation at 
		/// <a href = "http://docs.cloudant.com/api/cloudant-query.html#cloudant-query-sort-syntax">
		/// http://docs.cloudant.com/api/cloudant-query.html#cloudant-query-sort-syntax</a>
		/// </summary>
		/// <param name="fieldName">The name of the field.</param>
		public IndexField (string fieldName)
		{
			this.name = fieldName;
		}

		/// <summary>
		/// Create an IndexField
		/// </summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="sortOrder">The sort order (ascending|descending)</param>
		public IndexField(String fieldName, SortOrder sortOrder){
			this.name = fieldName;
			this.sortOrder = sortOrder;
		}
	}
}

