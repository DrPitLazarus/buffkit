using System.Collections.Generic;
using UnityEngine;

namespace BuffKit.Broadcast
{
    public class MatchDataObserver : MonoBehaviour
    {
        public static MatchDataObserver Instance { get; private set; }

        public void AddKillfeedEntry(string entry)
        {
            _killfeedList.Add(entry);
        }

        private List<string> _killfeedList;

        private Transform _shipContainer;
        protected virtual void Awake()
        {
            if (Instance != null)
                MuseLog.Error("MatchDataObserver already exists, new one should not have been created");
            _shipContainer = transform.Find("Ship Container");
            _killfeedList = new List<string>();
            Instance = this;
        }
        protected virtual void Update()
        {
            var ships = new List<Airship>();
            for (var i = 0; i < _shipContainer.childCount; i++)
            {
                var potentialShip = _shipContainer.GetChild(i);
                var ship = potentialShip.GetComponent<Airship>();
                if (ship != null && ship.IsHumanControlled)
                    ships.Add(ship);
            }

            if (ships.Count == 0 || MatchLobbyView.Instance == null || Mission.Instance == null) return;

            MatchData data = new MatchData(ships, _killfeedList);
            _killfeedList.Clear();

            Broadcaster.Instance.SendMatchData(data);
        }
    }
}
