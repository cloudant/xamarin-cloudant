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


namespace Com.Cloudant.Client
{
	/// <summary>
	/// DocumentRevision is a class representing a document revision.
	/// </summary>
	/// <remarks>
	/// This class contains the metadata and content associated with a Cloudant document revision.
	/// </remarks>
	public class DocumentRevision
	{
		private static string DOC_ID = "_id";
		private static string DOC_REV = "_rev";
		private static string DOC_DELETED = "_deleted";

		/// <summary>
		/// Gets or sets the document identifier.
		/// </summary>
		/// <value>The document identifier.</value>
		public string docId { get; set;}

		/// <summary>
		/// Gets or sets the revision identifier.
		/// </summary>
		/// <value>The revision identifier.</value>
		public string revId { get; set;}

		/// <summary>
		/// Gets or sets the document body.
		/// </summary>
		/// <value>The document body.</value>
		public Dictionary<String,Object> body { get; set;}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Com.Cloudant.Client.DocumentRevision"/> is deleted.
		/// </summary>
		/// <value><c>true</c> if is deleted; otherwise, <c>false</c>.</value>
		public Boolean isDeleted { get; set;}

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Cloudant.Client.DocumentRevision"/> class.
		/// </summary>
		public DocumentRevision ()
		{
			docId = null;
			revId = null;
			isDeleted = false;
			body = new Dictionary<String,Object> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Cloudant.Client.DocumentRevision"/> class with document contents.
		/// </summary>
		/// <param name="documentId">Document identifier.</param>
		/// <param name="revisionId">Revision identifier.</param>
		/// <param name="documentBody">Document body.</param>
		public DocumentRevision (String documentId, String revisionId, Dictionary<String,Object> documentBody)
		{
			docId = documentId;
			revId = revisionId;
			body = documentBody;

			// remove meta-data from body
			if (body.ContainsKey (DOC_ID)) {
				body.Remove (DOC_ID);
			}
			if (body.ContainsKey (DOC_REV)) {
				body.Remove (DOC_REV);
			}
			if (body.ContainsKey (DOC_DELETED)) {
				Object deleted;
				body.TryGetValue (DOC_DELETED, out deleted);
				isDeleted = (Boolean)deleted;
				body.Remove (DOC_DELETED);
			}
		}
	}
}
