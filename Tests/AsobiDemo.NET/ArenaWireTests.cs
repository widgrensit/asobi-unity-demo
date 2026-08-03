using AsobiDemo;
using NUnit.Framework;

namespace AsobiDemo.Tests
{
    [TestFixture]
    public class ArenaWireTests
    {
        // Shape pinned by asobi's priv/protocol/fixtures/match.finished.json:
        // the game's result table is nested under payload.result.
        const string MatchFinished =
            "{\"type\":\"match.finished\",\"payload\":{\"match_id\":\"01j8x1\",\"result\":" +
            "{\"status\":\"completed\",\"winner\":\"p1\",\"round\":3,\"standings\":[" +
            "{\"player_id\":\"p1\",\"kills\":7,\"deaths\":2,\"rank\":1}," +
            "{\"player_id\":\"p2\",\"kills\":4,\"deaths\":5,\"rank\":2}]}}}";

        const string PlayingState =
            "{\"type\":\"match.state\",\"payload\":{\"phase\":\"playing\",\"round\":2," +
            "\"modifier\":\"low_gravity\",\"time_remaining\":45000," +
            "\"players\":{\"p1\":{\"x\":120.5,\"y\":80,\"hp\":75,\"kills\":3,\"deaths\":1}}," +
            "\"projectiles\":[{\"id\":9,\"x\":10,\"y\":20,\"owner\":\"p1\"}]," +
            "\"my_boons\":[\"rapid_fire\"]}}";

        const string BoonPickState =
            "{\"phase\":\"boon_pick\",\"time_remaining\":8000,\"picks_done\":[\"p1\",\"p2\"]," +
            "\"boon_offers\":[{\"id\":\"rapid_fire\",\"name\":\"Rapid Fire\",\"description\":\"Shoot faster\"}]}";

        static string Payload(string raw) => ArenaState.ExtractObject(raw, "payload");

        [Test]
        public void ExtractObjectPullsPayloadFromEnvelope()
        {
            var payload = Payload(MatchFinished);
            Assert.That(payload, Does.StartWith("{\"match_id\""));
            Assert.That(payload, Does.EndWith("}"));
        }

        [Test]
        public void ExtractObjectReturnsNullForMissingKey()
        {
            Assert.That(ArenaState.ExtractObject(MatchFinished, "nope"), Is.Null);
        }

        [Test]
        public void MatchResultReadsNestedResultTable()
        {
            var result = ArenaMatchResult.Parse(Payload(MatchFinished));

            Assert.That(result.status, Is.EqualTo("completed"));
            Assert.That(result.winner, Is.EqualTo("p1"));
            Assert.That(result.standings, Has.Count.EqualTo(2));
            Assert.That(result.standings[0].player_id, Is.EqualTo("p1"));
            Assert.That(result.standings[0].kills, Is.EqualTo(7));
            Assert.That(result.standings[1].deaths, Is.EqualTo(5));
            Assert.That(result.standings[1].rank, Is.EqualTo(2));
        }

        [Test]
        public void MatchResultAcceptsUnwrappedResultTable()
        {
            var result = ArenaMatchResult.Parse("{\"status\":\"completed\",\"winner\":\"p9\",\"standings\":[]}");

            Assert.That(result.winner, Is.EqualTo("p9"));
            Assert.That(result.standings, Is.Empty);
        }

        [Test]
        public void ParsesPlayingState()
        {
            var state = ArenaState.Parse(Payload(PlayingState));

            Assert.That(state.phase, Is.EqualTo("playing"));
            Assert.That(state.round, Is.EqualTo(2));
            Assert.That(state.modifier, Is.EqualTo("low_gravity"));
            Assert.That(state.time_remaining, Is.EqualTo(45000f));
            Assert.That(state.players["p1"].x, Is.EqualTo(120.5f));
            Assert.That(state.players["p1"].hp, Is.EqualTo(75));
            Assert.That(state.projectiles, Has.Count.EqualTo(1));
            Assert.That(state.projectiles[0].owner, Is.EqualTo("p1"));
            Assert.That(state.my_boons, Is.EqualTo(new[] { "rapid_fire" }));
        }

        [Test]
        public void PicksDoneCountsPlayerIdList()
        {
            var state = ArenaState.Parse(BoonPickState);

            Assert.That(state.phase, Is.EqualTo("boon_pick"));
            Assert.That(state.picks_done, Is.EqualTo(2));
            Assert.That(state.boon_offers[0].name, Is.EqualTo("Rapid Fire"));
        }

        [Test]
        public void PicksDoneStillAcceptsBareCount()
        {
            var state = ArenaState.Parse("{\"phase\":\"boon_pick\",\"picks_done\":3}");

            Assert.That(state.picks_done, Is.EqualTo(3));
        }
    }
}
