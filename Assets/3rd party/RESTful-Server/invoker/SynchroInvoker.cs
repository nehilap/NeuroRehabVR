// RESTful-Unity
// Copyright (C) 2016 - Tim F. Rieck
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//	You should have received a copy of the GNU General Public License
//	along with this program. If not, see <http://www.gnu.org/licenses/>.
//
// <copyright file="ServerInit.cs" company="TRi">
// Copyright (c) 2016 All Rights Reserved
// </copyright>
// <author>Tim F. Rieck</author>
// <date>29/11/2016 10:13 AM</date>

using UnityEngine;
using System;
using System.Net;
using RESTfulHTTPServer.src.models;
using RESTfulHTTPServer.src.controller;

namespace RESTfulHTTPServer.src.invoker
{
	public class SynchroInvoker
	{
		private const string TAG = "SynchroInvoker";

		/// <summary>
		/// Get the color of an object
		/// </summary>
		/// <returns>The color.</returns>
		/// <param name="request">Request.</param>
		public static Response SyncCall(Request request)
		{
			Response response = new Response();
			string responseData = "";

			// Verbose all URL variables
			foreach(string key in request.GetQuerys().Keys) {
				string value = request.GetQuery(key);
				RESTfulHTTPServer.src.controller.Logger.Log(TAG, "key: " + key + " , value: " + value);
			}

			UnityInvoker.ExecuteOnMainThread.Enqueue(() => {  

				Debug.Log("Sync call received");
				responseData = "ok";
				/*
				// 404 - Not found
				responseData = "404";
				response.SetContent(responseData);
				response.SetHTTPStatusCode((int) HttpStatusCode.NotFound);
				response.SetMimeType(Response.MIME_CONTENT_TYPE_TEXT);
				*/
			});

			// Wait for the main thread
			while (responseData.Equals ("")) {}

			// 200 - OK
			// Fillig up the response with data
			response.SetContent(responseData);
			response.SetMimeType(Response.MIME_CONTENT_TYPE_JSON);

			return response;
		}
	}
}

