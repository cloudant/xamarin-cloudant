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

namespace IBM.Cloudant.Client
{
	/// <summary>
	/// Class to encapsulate options for FindByIndex operations.
	/// </summary>
	/// <remarks>
	/// This class is used for specifying options used in a FindByIndex database query.
	/// </remarks>
	public class FindByIndexOptions
	{
		// search fields
		private Int32 plimit;
		private Int32 pskip;
		private List<IndexField> psort = new List<IndexField>();
		private List<String> pfields = new List<String>();
		private Int32 preadQuorum;
		private String puseIndex = null;


		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Cloudant.Client.Model.FindByIndexOptions"/> class.
		/// </summary>
		public FindByIndexOptions ()
		{
		}


		/// <summary>
		/// limit the number of results return
		/// </summary>
		/// <param name="limit">Max number of results</param>
		/// <returns>FindByIndexOptions instance with 'limit' parameter set.</returns>
		public FindByIndexOptions limit(Int32 limit) {
			this.plimit = limit;
			return this;
		}


		/// <summary>
		/// Skips <i>n</i> number of results.
		/// </summary>
		/// <param name="skip">Number of results to skip</param>
		/// <returns>FindByIndexOptions instance with 'skip' parameter set.</returns>
		public FindByIndexOptions skip(Int32 skip) {
			this.pskip = skip;
			return this;
		}


		/// <summary>
		/// Sets the readQuorum
		/// </summary>
		/// <param name="readQuorum">The read quorum</param>
		/// <returns>FindByIndexOptions instance with 'readQuorum' parameter set.</returns>
		public FindByIndexOptions readQuorum(Int32 readQuorum) {
			this.preadQuorum = readQuorum;
			return this;
		}


		/// <summary>
		/// Can be called multiple times to set the list of return fields
		/// </summary>
		/// <param name="field">set the return fields</param>
		/// <returns>FindByIndexOptions instance with 'field' parameter added.</returns>
		public FindByIndexOptions fields(String field) {
			this.pfields.Add(field);
			return this;
		}

			
		/// <summary>
		/// Can be called multiple times to set the sort syntax
		/// </summary>
		/// <param name="sort">add a sort syntax field</param>
		/// <returns>FindByIndexOptions instance with 'sort' IndexField added.</returns>
		public FindByIndexOptions sort(IndexField sort) {
			this.psort.Add(sort);
			return this;
		}


		/// <summary>
		/// Specify a specific index to run the query against
		/// </summary>
		/// <param name="designDocument">set the design document to use</param>
		/// <returns>FindByIndexOptions instance with design document set.</returns>
		private FindByIndexOptions useIndex(String designDocument) {
			this.puseIndex = "\"" + designDocument + "\"";
			return this;
		}


		/// <summary>
		/// Specify a specific index to run the query against
		/// </summary>
		/// <returns>The index.</returns>
		/// <param name="designDocument">set the design document to use</param>
		/// <param name="indexName">set the index name to use</param>
		/// <returns>FindByIndexOptions instance with design document and index set.</returns>
		public FindByIndexOptions useIndex(String designDocument, String indexName) {
			this.puseIndex = "[\"" + designDocument + "\",\"" + indexName + "\"]";
			return this;
		}

		/// <summary>
		/// Get a list of the fields.
		/// </summary>
		/// <returns>List of fields as String class.</returns>
		public List<String> getFields() {
			return pfields;
		}

		/// <summary>
		/// Get a list of the sort fields.
		/// </summary>
		/// <returns>List of sort fields as IndexField class.</returns>
		public List<IndexField> getSort() {
			return psort;
		}

		/// <summary>
		/// Gets the limit.
		/// </summary>
		/// <returns>The limit setting as an integer.</returns>
		public Int32 getLimit() {
			return plimit;
		}

		/// <summary>
		/// Gets the skip.
		/// </summary>
		/// <returns>The skip setting as an integer.</returns>
		public Int32 getSkip() {
			return pskip;
		}

		/// <summary>
		/// Gets the read quorum.
		/// </summary>
		/// <returns>The read quorum setting as an integer.</returns>
		public Int32 getReadQuorum() {
			return preadQuorum;
		}

		/// <summary>
		/// Gets the name of the index to be used.
		/// </summary>
		/// <returns>The name of the index to be used as a String.</returns>
		public String getUseIndex() {
			return puseIndex;
		}


	}
}

