using Newtonsoft.Json.Linq;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts.Server.Extensions;
using System.Net;
using UnityEngine;
using Mirror;
using Enums;

/// <summary>
/// Contains Rest Requests Handler methods.
/// </summary>
public class RestRequestHandler : MonoBehaviour {

	private AnimationSettingsManager animSettingsManager;

	private void Start() {
		animSettingsManager = ObjectManager.Instance.getFirstObjectByName("AnimationSettingsManager")?.GetComponent<AnimationSettingsManager>();
	}

	[Server][SimpleEventServerRouting(HttpConstants.MethodPost, "/training/move")]
	public void PostTrainingMoveEndpoint(HttpListenerContext context) {
		// Debug.Log(context.Request.GetStringBody());
		bool returnVal = AnimationServerManager.Instance.moveArm();

		context.Response.JsonResponse(new JObject() {
			new JProperty("result:", returnVal)
		});
	}

	[Server][SimpleEventServerRouting(HttpConstants.MethodPost, "/move")]
	public void PostMoveEndpoint(HttpListenerContext context) {
		// Debug.Log(context.Request.GetStringBody());
		bool returnVal = false;

		if (AnimationServerManager.Instance.isTrainingRunning) {
			returnVal = false;
		} else {
			AnimationServerManager.Instance.RpcStartActualAnimation(false, "");
			returnVal = true;
		}

		context.Response.JsonResponse(new JObject() {
			new JProperty("result:", returnVal)
		});
	}

	[Server][SimpleEventServerRouting(HttpConstants.MethodPost, "/rest")]
	public void PostRestingEndpoint(HttpListenerContext context) {
		bool returnVal = false;

		if (AnimationServerManager.Instance.isTrainingRunning) {
			returnVal = false;
		} else {
			animSettingsManager.prevAnimType = animSettingsManager.animType;
			animSettingsManager.animType = AnimationType.Off;
			returnVal = true;

			animSettingsManager.spawnCorrectTarget(animSettingsManager.prevAnimType, animSettingsManager.animType);
			animSettingsManager.RpcSpawnCorrectTarget(animSettingsManager.prevAnimType, animSettingsManager.animType);
		}

		context.Response.JsonResponse(new JObject() {
			new JProperty("result:", returnVal)
		});
	}

	[Server][SimpleEventServerRouting(HttpConstants.MethodPost, "/spawn")]
	public void PostSpawnEndpoint(HttpListenerContext context) {
		bool returnVal = false;
		AnimationType _oldAnimType = animSettingsManager.animType;

		if (AnimationServerManager.Instance.isTrainingRunning) {
			returnVal = false;
		} else {
			animSettingsManager.animType = animSettingsManager.prevAnimType;
			returnVal = true;

			animSettingsManager.spawnCorrectTarget(_oldAnimType, animSettingsManager.animType);
			animSettingsManager.RpcSpawnCorrectTarget(_oldAnimType, animSettingsManager.animType);
		}

		context.Response.JsonResponse(new JObject() {
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

