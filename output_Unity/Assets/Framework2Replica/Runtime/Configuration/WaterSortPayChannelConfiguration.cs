using System;
using UnityEngine;

namespace WaterSort
{
    [Serializable]
    public sealed class WaterSortPayChannelInputConfig
    {
        [SerializeField] private string name;
        [SerializeField] private string format;
        public string Name => name;
        public string Format => format;

        public WaterSortPayChannelInputConfig(string name, string format)
        {
            this.name = name;
            this.format = format;
        }
    }

    public enum WaterSortAccountInputKind { Account = 0, Email = 1, Phone = 2, Pix = 3 }

    [Serializable]
    public sealed class WaterSortPayChannelConfig
    {
        [SerializeField] private int id;
        [SerializeField] private string displayName;
        [SerializeField] private WaterSortAccountInputKind inputKind;
        [SerializeField] private string accountPattern;
        [SerializeField] private WaterSortPayChannelInputConfig[] inputs;

        public int Id => id;
        private MahjongSlide.Withdrawal.MahjongFrameworkResources.PayChannel Resources =>
            MahjongSlide.Withdrawal.MahjongFrameworkResources.Active?.FindPayChannel(id);
        public string ImageName => Resources?.imageName ?? "N" + Math.Max(1, id);
        public string DisplayName => displayName;
        public WaterSortAccountInputKind InputKind => inputKind;
        public string AccountPattern => accountPattern;
        public Sprite Logo => Resources?.logo;
        public Sprite Icon => Resources?.Icon;
        public WaterSortPayChannelInputConfig[] Inputs =>
            inputs ?? Array.Empty<WaterSortPayChannelInputConfig>();

        public WaterSortPayChannelConfig(
            int id,
            params WaterSortPayChannelInputConfig[] inputs)
        {
            this.id = id;
            this.inputs = inputs ?? Array.Empty<WaterSortPayChannelInputConfig>();
        }
    }

    /// <summary>
    /// Exact managed counterpart of the native $bB.$PE() pay-channel table.
    /// Channel zero is the native sentinel entry; selectable channels are 1..25.
    /// </summary>
    public static class WaterSortPayChannelConfiguration
    {
        private static readonly WaterSortPayChannelConfig[] Configurations =
        {
            Channel(0, Input("acct", "str")),
            Channel(1, Input("acct", "str")),
            Channel(2, Input("acct", "str")),
            Channel(
                3,
                Input("acct", "str"),
                Input("name", "str"),
                Input("cpf", "d11")),
            Channel(
                4,
                Input("acct", "str"),
                Input("name", "str"),
                Input("cpf", "d11")),
            Channel(5, Input("acct", "str")),
            Channel(6, Input("acct", "str")),
            Channel(7, Input("acct", "str")),
            Channel(8, Input("acct", "str")),
            Channel(9, Input("acct", "str")),
            Channel(10, Input("acct", "str")),
            Channel(11, Input("acct", "str")),
            Channel(12, Input("acct", "str")),
            Channel(13, Input("acct", "str")),
            Channel(14, Input("acct", "str")),
            Channel(15, Input("acct", "str")),
            Channel(16, Input("acct", "str")),
            Channel(17, Input("acct", "str")),
            Channel(18, Input("acct", "str")),
            Channel(19, Input("acct", "str")),
            Channel(20, Input("acct", "str")),
            Channel(21, Input("acct", "str")),
            Channel(22, Input("acct", "str")),
            Channel(23, Input("acct", "str")),
            Channel(24, Input("acct", "str")),
            Channel(25, Input("acct", "str"))
        };

        public static WaterSortPayChannelConfig Get(int channel)
        {
            var configured = WaterSortCurrencyService.Database?.FindChannel(channel);
            if (configured != null) return configured;
            return channel >= 0 && channel < Configurations.Length
                ? Configurations[channel]
                : null;
        }

        private static WaterSortPayChannelConfig Channel(
            int id,
            params WaterSortPayChannelInputConfig[] inputs)
        {
            return new WaterSortPayChannelConfig(id, inputs);
        }

        private static WaterSortPayChannelInputConfig Input(string name, string format)
        {
            return new WaterSortPayChannelInputConfig(name, format);
        }
    }
}

