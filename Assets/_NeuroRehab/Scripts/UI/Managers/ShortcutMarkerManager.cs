using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShortcutMarkerManager : MonoBehaviour {
	[SerializeField] private GameObject shortcutMarkerPrefab;

	private List<string> shortcutMarkerItems = new List<string>();
	private List<GameObject> spawnedShortcutMarkers = new List<GameObject>();

	public void addMarker(string text) {
		if (shortcutMarkerItems.Contains(text)) {
			return;
		}
		shortcutMarkerItems.Add(text);
		initShortcutMarkers();
	}

	public void removeMarker(string text) {
		if (!shortcutMarkerItems.Contains(text)) {
			return;
		}

		shortcutMarkerItems.Remove(text);
		initShortcutMarkers();
	}

	public void triggerMarker(string text) {
		if (shortcutMarkerItems.Contains(text)) {
			shortcutMarkerItems.Remove(text);
		} else {
			shortcutMarkerItems.Add(text);
		}
		initShortcutMarkers();
	}

	private void initShortcutMarkers() {
		foreach (GameObject item in spawnedShortcutMarkers) {
			GameObject.Destroy(item);
		}
		spawnedShortcutMarkers.Clear();

		for (int i = 0; i < shortcutMarkerItems.Count; i++) {
			GameObject newShortcutMarker = GameObject.Instantiate(shortcutMarkerPrefab, this.transform) as GameObject;
			spawnedShortcutMarkers.Add(newShortcutMarker);

			RectTransform rectTransform = newShortcutMarker.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(0, ((i + 1) * rectTransform.sizeDelta.y) + i * 5f);

			newShortcutMarker.GetComponentInChildren<TMP_Text>().text = shortcutMarkerItems[i];
		}
	}
}
