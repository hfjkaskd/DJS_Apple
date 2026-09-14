using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Logic
{
	public class AppServiceHelper : MonoBehaviour
	{
		private static AppServiceHelper instance;

		private static int OnSystemActionCallCount;

		private static List<Action<bool>> OnSystemActionList;

		private static List<global::JIKLKHJMDHP<bool>> OnSystemActionSortedList;

		private static List<global::JIKLKHJMDHP<bool>> OnSystemActionFIFOSortedList;

		private static DateTime lastUtcTickTime;

		private static DateTime lastLocalTickTime;

		private static List<global::JIKLKHJMDHP<int>> OnUtcDayActionSortedList;

		private static List<global::JIKLKHJMDHP<int>> OnLocalDayActionSortedList;

		private static List<MPIFBEIHPFG> OnInputTouchList;

		private static float timeForCheckNetwork;

		private static bool isInternectConnect;

		public static void Init()
		{
		}

		public static void AddOnUtcDayChanged(string actionName, Action<int> mOnChanged, int sortingOrder)
		{
		}

		public static void AddOnLocalDayChanged(string actionName, Action<int> mOnChanged, int sortingOrder)
		{
		}

		public static void RemoveOnUtcDayChanged(string actionName)
		{
		}

		public static void RemoveOnLocalDayChanged(string actionName)
		{
		}

		public static void OnTimeTick()
		{
		}

		public static void AddAppPauseEvent(string actionName, Action<bool> mOnSystem, int sortingOrder)
		{
		}

		public static void RemoveAppPauseEvent(string actionName)
		{
		}

		public static void AddAppPauseEvent(Action<bool> mOnSystem)
		{
		}

		public static void RemoveAppPauseEvent(Action<bool> mOnSystem)
		{
		}

		private void OnPauseAction(bool pauseStatus)
		{
		}

		public void OnApplicationPauseAction(bool pauseStatus)
		{
		}

		private void OnApplicationQuitAction()
		{
		}

		public static void AddOnInputTouch(string actionName, Action action)
		{
		}

		private void Update()
		{
		}

		public void StartCheckInternetConnect()
		{
		}

		public static void SetIsInternetConnect(bool value)
		{
		}

		public static bool CheckInternetConnect()
		{
			return false;
		}
	}
}
