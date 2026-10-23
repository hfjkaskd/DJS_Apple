using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Spine;

// Runs outside Unity against the built player DLLs. No substitute implementation
// is used: this verifies JSON persistence and actual reference skeleton decoding.
internal static class DependencySmoke
{
    private sealed class HistoryEntry
    {
        [JsonProperty("money")] public double Money;
        [JsonProperty("stage")] public int Stage;
        [JsonProperty("note")] public string Note;
    }

    private sealed class DecodeAttachments : AttachmentLoader
    {
        private static readonly AtlasRegion Region = new AtlasRegion {
            width = 64, height = 64, originalWidth = 64, originalHeight = 64,
            page = new AtlasPage { width = 64, height = 64 }
        };
        public RegionAttachment NewRegionAttachment(Skin skin, string name, string path, Sequence sequence)
            => new RegionAttachment(name) { Region = Region, Sequence = sequence };
        public MeshAttachment NewMeshAttachment(Skin skin, string name, string path, Sequence sequence)
            => new MeshAttachment(name) { Region = Region, Sequence = sequence };
        public BoundingBoxAttachment NewBoundingBoxAttachment(Skin skin, string name) => new BoundingBoxAttachment(name);
        public PathAttachment NewPathAttachment(Skin skin, string name) => new PathAttachment(name);
        public PointAttachment NewPointAttachment(Skin skin, string name) => new PointAttachment(name);
        public ClippingAttachment NewClippingAttachment(Skin skin, string name) => new ClippingAttachment(name);
    }

    private static int Main(string[] args)
    {
        var expected = new List<HistoryEntry> { new HistoryEntry { Money = .1, Stage = 2, Note = "历史 / local test" } };
        string json = JsonConvert.SerializeObject(expected);
        var actual = JsonConvert.DeserializeObject<List<HistoryEntry>>(json);
        if (string.IsNullOrEmpty(json) || actual == null || actual.Count != 1 || actual[0].Money != .1 ||
            actual[0].Stage != 2 || actual[0].Note != expected[0].Note)
            throw new InvalidOperationException("Genuine Json.NET history round-trip failed.");
        Console.WriteLine("JSON history round-trip: PASS " + json);
        int count = 0;
        foreach (string path in Directory.GetFiles(args[0], "*.json"))
        {
            string text = File.ReadAllText(path);
            if (!text.Contains("\"skeleton\"") || !text.Contains("\"spine\"")) continue;
            var reader = new SkeletonJson(new DecodeAttachments());
            SkeletonData data;
            using (var stream = new StringReader(text)) data = reader.ReadSkeletonData(stream);
            if (data == null || data.Bones.Count == 0 || data.Animations.Count == 0)
                throw new InvalidOperationException("Skeleton data did not decode: " + path);
            var skeleton = new Skeleton(data);
            var animation = new AnimationState(new AnimationStateData(data));
            animation.SetAnimation(0, data.Animations.Items[0], true);
            animation.Update(.25f);
            if (!animation.Apply(skeleton) || animation.GetCurrent(0).TrackTime <= 0)
                throw new InvalidOperationException("Skeleton animation did not run: " + path);
            skeleton.UpdateWorldTransform(Skeleton.Physics.Update);
            Console.WriteLine("SPINE " + Path.GetFileName(path) + ": PASS " + data.Version +
                " bones=" + data.Bones.Count + " animations=" + data.Animations.Count);
            count++;
        }
        if (count < 5) throw new InvalidOperationException("Expected the five imported Framework 2 skeleton JSON files.");
        return 0;
    }
}
