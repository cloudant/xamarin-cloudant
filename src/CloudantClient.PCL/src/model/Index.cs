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
using System.Collections.Generic;
using System.Collections;

namespace IBM.Cloudant.Client
{
	/// <summary>
	/// Encapsulates a Cloudant Index definition.
	/// Additional information about indexes in the Cloudant documentation at: 
	/// <a href="https://docs.cloudant.com/cloudant_query.html#creating-an-index">https://docs.cloudant.com/cloudant_query.html#creating-an-index</a>
	/// </summary>
	/// <remarks>
	/// This class contains metadata and information for a Cloudant Index.
	/// </remarks>
	public class Index
	{
		/// <summary>
		/// Design document identifier for this index
		/// </summary>
		public string ddoc { get; }

		/// <summary>
		/// Name of the index
		/// </summary>
		public string name { get; }

		/// <summary>
		/// Index type e.g. json
		/// </summary>
		public string type { get; }


		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Cloudant.Client.Model.Index"/> class.
		/// </summary>
		/// <param name="designDocId">Design document identifier</param>
		/// <param name="name">Name of the index</param>
		/// <param name="type">Index type e.g. json</param>
		public Index(string designDocId, string name, string type) {
			this.ddoc = designDocId;
			this.name = name;
			this.type = type;
		}


		/// <summary>
		/// Adds a new index field
		/// </summary>
		/// <param name="fieldName">Name of the index field</param>
		/// <param name="order">Sort order</param>
		public void AddIndexField(string fieldName, IndexField.SortOrder order) {
			indexFields.Add(new IndexField(fieldName, order));
		}

		//NOTE: The declaration of indexFields should be kept at the end of the file because this 
		//      statement produces a validation error in the XML help of any element defined after it.

		/// <summary>
		/// Index Fields for this index 
		/// </summary>
		public List<IndexField> indexFields { get; } = new List<IndexField>();
	}
}

