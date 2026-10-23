using System;
using System.Collections.Generic;
using UnityEngine;

namespace WZSDK
{
	public class GameSceneWZ : MonoBehaviour
	{
		private float dt;

		private void Awake()
		{
			GameApp.Instance.Init();
		}

		private void Start()
		{
		}

		private void Update()
		{
			dt = Time.deltaTime;
			GameApp.Instance.Update(dt);
		}

		public void withDraw(
			int level,
			double money,
			int closeType,
			ViewType viewType,
			string historyId,
			int stageId = 0)
		{
			int stage = stageId;
			if (stage < 1 && GameManagerWZ.instance != null)
			{
				stage = GameManagerWZ.instance.currentStage;
			}
			if (stage < 2)
			{
				stage = 1;
			}

			DataModel dataModel = new DataModel
			{
				Level = level,
				Num = stage,
				Money = money,
				HistoryId = historyId,
				CloseType = closeType
			};
			EnsureViewManager();
			Framework2BusinessAnalytics.Withdraw("stage_withdraw_requested", dataModel, "open", (int)viewType);
			GameApp.viewManager?.Open(viewType, dataModel);
		}

		public void withDrawTem(
			int level,
			double money,
			int closeType,
			int type)
		{
			int stage = GameManagerWZ.instance != null
				? GameManagerWZ.instance.currentStage
				: 1;
			if (stage < 2)
			{
				stage = 1;
			}

			DataModel dataModel = new DataModel
			{
				Level = level,
				Num = stage,
				Money = money,
				Type = type,
				CloseType = closeType
			};
			EnsureViewManager();
			GameApp.viewManager?.Open(
				ViewType.WithDraw, dataModel);
		}

		public string setHistory(double money, int stage)
		{
			List<HistoryModel> history =
				SPlayerPrefs.GetListTem<HistoryModel>(
					FacadePlayerPrefExtend.History);
			if (history == null)
			{
				history = new List<HistoryModel>();
			}

			DateTime today = Framework2Host.LocalNow.Date;
			HistoryModel item = new HistoryModel
			{
				DateTime = today.Month + "/" +
					today.Day + "/" + today.Year,
				Money = money,
				ID = Guid.NewGuid().ToString(),
				Stage = stage,
				Status = 2
			};
			history.Add(item);
			SPlayerPrefs.SetListTem(
				FacadePlayerPrefExtend.History, history);
			MahjongProductAnalytics.Track("f2_legacy_history_created", "history_id", MahjongProductAnalytics.Id(item.ID), "stage", item.Stage, "status", item.Status, "amount", item.Money, "display_date", item.DateTime, "actual_payment", false);
			return item.ID;
		}

		private static void EnsureViewManager()
		{
			if (GameApp.viewManager == null)
			{
				GameApp.Instance.Init();
			}
		}
	}
}
