using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    float globalTimer = 0f;
    int globalExports = 0;

    // public string id;

    InterfaceManager interfaceManager;

    public Dictionary<string, Crate> cratesHolder = new Dictionary<string, Crate>();

    void Start()
    {
        interfaceManager.GetComponent<InterfaceManager>();
    }

    void Update()
    {
        globalTimer += Time.deltaTime;
    }

    // public void SetId(string _id)
    // {
    //     id = _id;
    // }

    public void SetupGameState()
    {
        var crates = GameObject.FindGameObjectsWithTag("Crate");
        foreach (var _crate in crates)
        {
            string id = _crate.GetComponent<CrateSetter>().Id;
            Crate crate = new Crate(id, AccessCode.All);
            cratesHolder.Add(id, crate);
        }

        var ccrates = GameObject.FindGameObjectsWithTag("Cop Crate");
        foreach (var _crate in crates)
        {
            string id = _crate.GetComponent<CrateSetter>().Id;
            Crate crate = new Crate(id, AccessCode.Cops);
            cratesHolder.Add(id, crate);
        }

        var rcrates = GameObject.FindGameObjectsWithTag("Rob Crate");
        foreach (var _crate in crates)
        {
            string id = _crate.GetComponent<CrateSetter>().Id;
            Crate crate = new Crate(id, AccessCode.Robs);
            cratesHolder.Add(id, crate);
        }
    }

    public bool UpdateGameState(PlayerPacket packet, OtherController oc)
    {
        // ValidateAction(oc.gameObject);
        if (oc.crateList.Count == 0) return false;
        if (cratesHolder.TryGetValue(oc.crateList[oc.crateList.Count - 1], out Crate crate) && cratesHolder.TryGetValue(packet.Token, out Crate player))
        {
            var reaction = crate.DoAction(packet.Action, player.Access);
            if (reaction != ActionType.Null)
            {
                player.DoAction(reaction, player.Access);
            }
            return true;
        }
        return false;
    }

    // bool ValidateAction(GameObject gameObject)
    // {
    //     var colliders = Physics.OverlapSphere(gameObject.transform.position, 0.0f);
    //     return false;
    // }
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

public enum StateType
{
    Game,
    Score,
    Other
}

public enum ActionType
{
    Null,
    GetDrugs,
    StoreDrugs,
    GetEvidence,
    StoreEvidence,
    CreateEvidence,
    DestroyEvidence,
    CreateWarrant,
    UseWarrant,
    CreateJail,
}

public class Crate
{
    public readonly string Id;
    public readonly AccessCode Access;
    public int Drugs { get; private set; }
    public int Evidence { get; private set; }

    public Crate(string id, AccessCode access)
    {
        Id = id;
        Access = access;
        Evidence = 0;
        Drugs = 0;
    }

    public ActionType DoAction(ActionType action, AccessCode otherAccess)
    {
        if (otherAccess == Access)
        {
            if (action == ActionType.GetDrugs && Drugs > 0)
            {
                Drugs -= 1;
                return ActionType.StoreDrugs;
            }
            if (action == ActionType.StoreDrugs && Drugs < 100)
            {
                Drugs += 1;
                DoAction(ActionType.CreateEvidence, otherAccess);
                return ActionType.GetDrugs;
            }
            if (action == ActionType.GetEvidence && Evidence > 0)
            {
                Evidence -= 1;
                return ActionType.StoreEvidence;
            }
            if (action == ActionType.StoreEvidence && Evidence < 2)
            {
                Evidence += 1;
                return ActionType.GetEvidence;
            }
            if (action == ActionType.CreateEvidence && Evidence < 2)
            {
                Evidence += 1;
                return ActionType.Null;
            }
            if (action == ActionType.DestroyEvidence && Evidence > 0)
            {
                Evidence -= 1;
                return ActionType.Null;
            }
        }
        return ActionType.Null;
    }

    public ActionType DoSkill(ActionType action, AccessCode otherAccess)
    {
        // if (otherAccess == Access)
        // {
        //     if (action == ActionType.GetDrugs && Drugs > 0)
        //     {
        //         Drugs -= 1;
        //         return ActionType.StoreDrugs;
        //     }
        //     if (action == ActionType.StoreDrugs && Drugs < 100)
        //     {
        //         Drugs += 1;
        //         DoAction(ActionType.CreateEvidence, otherAccess);
        //         return ActionType.GetDrugs;
        //     }
        //     if (action == ActionType.GetEvidence && Evidence > 0)
        //     {
        //         Evidence -= 1;
        //         return ActionType.StoreEvidence;
        //     }
        //     if (action == ActionType.StoreEvidence && Evidence < 2)
        //     {
        //         Evidence += 1;
        //         return ActionType.GetEvidence;
        //     }
        //     if (action == ActionType.CreateEvidence && Evidence < 2)
        //     {
        //         Evidence += 1;
        //         return ActionType.Null;
        //     }
        //     if (action == ActionType.DestroyEvidence && Evidence > 0)
        //     {
        //         Evidence -= 1;
        //         return ActionType.Null;
        //     }
        // }
        return ActionType.Null;
    }
}