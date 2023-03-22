using Newtonsoft.Json.Linq;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server.Extensions;
using System.Net;
using UnityEngine;

public class RestRequestHandler : MonoBehaviour {

	[SimpleEventServerRouting(HttpConstants.MethodPost, "/move")]
	public void PostRawStringEndpoint(HttpListenerContext context) {
		// Debug.Log(context.Request.GetStringBody());

		bool returnVal = AnimationServerManager.Instance.moveArm();

		context.Response.JsonResponse(new JObject()	{
			new JProperty("result:", returnVal)
		});
	}
/*
	[SimpleEventServerRouting(HttpConstants.MethodPost, "/json")]
	public void PostJsonEndpoint(HttpListenerContext context)
	{
		var jsonObject = context.Request.GetJsonBody<AnimationJsonBody>();
		if (jsonObject == null)
		{
			Debug.Log("Invalid Body!");
			return;
		}

		string serializedObject = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
		Debug.Log(serializedObject);

		context.Response.JsonResponse(new JObject()	{
			new JProperty("result:", "true")
		});

		Debug.Log(DateTime.Now);
	}

	public struct AnimationJsonBody {
		public string animationType;
		public int time;
	}
 */
}

