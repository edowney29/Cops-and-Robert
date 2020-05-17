using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    NetworkManager networkManager;

    public Dictionary<string, Crate> cratesList = new Dictionary<string, Crate>();

    void Start()
    {
        networkManager = GetComponent<NetworkManager>();

        var crates = GameObject.FindGameObjectsWithTag("Crates");
        foreach (var _crate in crates)
        {
            string id = System.Guid.NewGuid().ToString();
            Crate crate = new Crate(id, AccessCode.All);
        }

        var ccrates = GameObject.FindGameObjectsWithTag("CopsCrates");
        foreach (var _crate in crates)
        {
            string id = System.Guid.NewGuid().ToString();
            Crate crate = new Crate(id, AccessCode.Cops);
        }

        var rcrates = GameObject.FindGameObjectsWithTag("RobsCrates");
        foreach (var _crate in crates)
        {
            string id = System.Guid.NewGuid().ToString();
            Crate crate = new Crate(id, AccessCode.Robs);
        }
    }

    void Update()
    {

        var colliders = Physics.OverlapSphere(transform.position, 0.0f);
    }

    public enum AccessCode
    {
        All,
        Cops,
        Robs
    }

    public enum RoleCode
    {
        _1,
        _2,
        _3,
        _4,
        _5,
    }

    public struct PlayerType
    {
        public AccessCode accessCode;
        public RoleCode roleCode;
    }

    public class Crate
    {
        public Crate(string id, AccessCode access)
        {
            Id = id;
            Access = access;
            Drugs = 0;
            Evidence = 0;
        }

        public string Id { get; private set; }
        public AccessCode Access { get; private set; }
        public int Drugs
        {
            get
            {
                return Drugs;
            }
            private set
            {
                Drugs = value;
                DrugsChanged(this);
            }
        }
        public int Evidence
        {
            get
            {
                return Evidence;
            }
            private set
            {
                Evidence = value;
                EvidenceChanged(this);
            }
        }

        public event System.Action<Crate> DrugsChanged;
        public event System.Action<Crate> EvidenceChanged;
    }
}
