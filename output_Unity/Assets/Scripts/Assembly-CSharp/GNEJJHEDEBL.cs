using System;
using System.Collections.Generic;
using PlayerModel;

/// <summary>玩家资产模型：金币与道具库存。默认 Coin=100、四道具各2。</summary>
public class GNEJJHEDEBL : global::IAJDGNOGFGO<PlayerAssetSaveData, GNEJJHEDEBL>
{
	private Dictionary<POJCEPBNNIP, LCDMPNNLNEA> ItemTypeConfigDict;

	public static event Action<POJCEPBNNIP> OnItemChanged;

	public PlayerAssetSaveData PlayerAssetData => data;

	public GNEJJHEDEBL()
	{
		ItemTypeConfigDict = new Dictionary<POJCEPBNNIP, LCDMPNNLNEA>
		{
			{ POJCEPBNNIP.Undo, new LCDMPNNLNEA(POJCEPBNNIP.Undo, 2, 100) },
			{ POJCEPBNNIP.Shuffle, new LCDMPNNLNEA(POJCEPBNNIP.Shuffle, 3, 150) },
			{ POJCEPBNNIP.Magic, new LCDMPNNLNEA(POJCEPBNNIP.Magic, 4, 200) },
			{ POJCEPBNNIP.Extra, new LCDMPNNLNEA(POJCEPBNNIP.Extra, 5, 300) }
		};
	}

	protected override string GetKey()
	{
		return "PlayerAssetModel";
	}

	public override void Init()
	{
		base.Init();
		if (data.playerProperties == null)
		{
			data.playerProperties = new List<PropertyInfo>();
			InitDefaultItem();
		}
	}

	protected override void AfterFirstInitData()
	{
		data.playerProperties = new List<PropertyInfo>();
		InitDefaultItem();
	}

	private void InitDefaultItem()
	{
		InitItemCount(POJCEPBNNIP.Coin, 100);
		InitItemCount(POJCEPBNNIP.Undo, 2);
		InitItemCount(POJCEPBNNIP.Shuffle, 2);
		InitItemCount(POJCEPBNNIP.Magic, 2);
		InitItemCount(POJCEPBNNIP.Extra, 2);
	}

	private void InitItemCount(POJCEPBNNIP InItemType, int count)
	{
		PropertyInfo info = GetPropertyInfo(InItemType);
		info.count = count;
		info.tempCount = 0;
	}

	public void CleanTempPropertyByType(POJCEPBNNIP InItemType)
	{
		PropertyInfo info = GetPropertyInfo(InItemType);
		info.tempCount = 0;
		SaveData();
		OnItemChanged?.Invoke(InItemType);
	}

	public int GetTotalCoinIncome()
	{
		return data.totalGoldIncome;
	}

	public int GetTotalCoinSpend()
	{
		return data.totalGoldSpend;
	}

	public void UseItem(POJCEPBNNIP InItemType, int InCount, DPOCDHPGHMF InItemSpendReason, float InUseProgress = 0f, int InUseItems = 0)
	{
		PropertyInfo info = GetPropertyInfo(InItemType);
		int remain = InCount;
		if (info.tempCount > 0)
		{
			int useTemp = Math.Min(info.tempCount, remain);
			info.tempCount -= useTemp;
			remain -= useTemp;
		}
		info.count = Math.Max(0, info.count - remain);
		if (InItemType == POJCEPBNNIP.Coin)
		{
			data.totalGoldSpend += InCount;
		}
		SaveData();
		OnItemChanged?.Invoke(InItemType);
	}

	public void AddItem(POJCEPBNNIP InItemType, int InCount, bool InIsAddTemp, NMNPFKEJMHO InItemAddSource)
	{
		PropertyInfo info = GetPropertyInfo(InItemType);
		if (InIsAddTemp)
		{
			info.tempCount += InCount;
		}
		else
		{
			info.count += InCount;
		}
		if (InItemType == POJCEPBNNIP.Coin)
		{
			data.totalGoldIncome += InCount;
		}
		SaveData();
		OnItemChanged?.Invoke(InItemType);
	}

	public int GetItemCount(POJCEPBNNIP InItemType, bool isAddTemp)
	{
		PropertyInfo info = GetPropertyInfo(InItemType);
		return isAddTemp ? info.count + info.tempCount : info.count;
	}

	public void SetItem(POJCEPBNNIP InItemType, int count)
	{
		PropertyInfo info = GetPropertyInfo(InItemType);
		info.count = count;
		SaveData();
		OnItemChanged?.Invoke(InItemType);
	}

	private PropertyInfo GetPropertyInfo(POJCEPBNNIP InItemType)
	{
		if (data.playerProperties == null)
		{
			data.playerProperties = new List<PropertyInfo>();
		}
		foreach (PropertyInfo p in data.playerProperties)
		{
			if (p.itemId == InItemType)
			{
				return p;
			}
		}
		PropertyInfo created = new PropertyInfo { itemId = InItemType };
		data.playerProperties.Add(created);
		return created;
	}

	public int GetItemUnlockLevel(POJCEPBNNIP InType)
	{
		return ItemTypeConfigDict.TryGetValue(InType, out LCDMPNNLNEA config) ? config.UnLockLevel : 0;
	}

	public int GetItemGoldCost(POJCEPBNNIP InType)
	{
		return ItemTypeConfigDict.TryGetValue(InType, out LCDMPNNLNEA config) ? config.ItemGoldCost : 0;
	}
}
