﻿using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int exportScore = 0;
    float gameTimer = 0f, tickTimer = 0f;
    bool isRunning = false;

    // List<string> exportCrates = new List<string>();
    // List<float> exportTimers = new List<float>();

    Dictionary<string, List<float>> exportHolder = new Dictionary<string, List<float>>();
    Dictionary<string, Crate> cratesHolder = new Dictionary<string, Crate>();

    void Update()
    {
        if (!isRunning)
        {
            exportScore = 0;
            gameTimer = 0f;
            tickTimer = 0f;
        }
        else
        {
            gameTimer += Time.deltaTime;
            tickTimer += Time.deltaTime;

            foreach (var export in exportHolder)
            {
                if (cratesHolder.TryGetValue(export.Key, out Crate crate))
                {
                    export.Value.RemoveAll(timer =>
                    {
                        timer += Time.deltaTime;
                        return crate.ScoreDrug(timer);
                    });
                }
            }
        }
    }

    public void ResetGameState()
    {
        CancelInvoke("UpdateCrateTimers");
        isRunning = false;
    }

    public void SetupGameState(Dictionary<string, OtherController> players, string _id)
    {
        cratesHolder.Clear();
        var names = new PlayerNameSetter().Names;
        Crate mycrate = new Crate(_id, names[names.Length - 1]);
        cratesHolder.Add(_id, mycrate);

        int index = 0;
        foreach (var player in players)
        {
            string id = player.Key;
            Crate crate = new Crate(player.Key, names[index++]);
            cratesHolder.Add(id, crate);
        }

        List<string> playerList = new List<string>();
        playerList.Add(_id);
        foreach (var key in cratesHolder.Keys)
        {
            playerList.Add(key);
        }

        for (int i = 0; i < playerList.Count; ++i)
        {
            string tmp = playerList[i];
            int rng = Random.Range(i, playerList.Count);
            playerList[i] = playerList[rng];
            playerList[rng] = tmp;
        }

        float rolePercent = Random.value;
        for (int i = 0; i < playerList.Count; i += 1)
        {
            if (cratesHolder.TryGetValue(playerList[i], out Crate player))
            {
                if (i == 0 || i == 1) player.Role = RoleCode._1;
                else if (i == 2 || i == 3) player.Role = RoleCode._2;
                else if (i == 4 || i == 5) player.Role = RoleCode._3;
                else if ((i == 6 || i == 7) && playerList.Count >= 8) player.Role = RoleCode._4;
                else if ((i == 8 || i == 9) && playerList.Count >= 10) player.Role = rolePercent > 0.5 ? RoleCode._5 : RoleCode._6;
                else player.Role = RoleCode._2;
            }
        }

        SetupCreates();
        InvokeRepeating("UpdateCrateTimers", 0f, 180f);
        isRunning = true;
    }

    void SetupCreates()
    {
        var crates = GameObject.FindGameObjectsWithTag("Crate");
        for (int i = 0; i < crates.Length; ++i)
        {
            GameObject tmp = crates[i];
            int rng = Random.Range(i, crates.Length);
            crates[i] = crates[rng];
            crates[rng] = tmp;
        }
        int idx = 0;
        foreach (var _crate in crates)
        {
            string id = _crate.GetComponent<CrateController>().Id;
            Crate crate = new Crate(id, "Crate");
            crate.Access = AccessCode.Null;
            if (idx == 0)
            {
                crate.IsExport = true;
                exportHolder.Add(crate.Id, new List<float>());
                crate.AddExportTimer += AddExportTimer;
                crate.RemoveExportTimer += RemoveExportTimer;

            }
            cratesHolder.Add(id, crate);
            idx += 1;
        }

        var rcrates = GameObject.FindGameObjectsWithTag("Rob Crate");
        foreach (var _crate in crates)
        {
            string id = _crate.GetComponent<CrateController>().Id;
            Crate crate = new Crate(id, "Drug Stash");
            crate.Access = AccessCode.Robs;
            cratesHolder.Add(id, crate);
        }

        var ccrates = GameObject.FindGameObjectsWithTag("Cop Crate");
        foreach (var _crate in crates)
        {
            string id = _crate.GetComponent<CrateController>().Id;
            Crate crate = new Crate(id, "Evidence Locker");
            crate.Access = AccessCode.Cops;
            cratesHolder.Add(id, crate);
        }
    }

    public bool UpdateGameState(PlayerPacket packet, OtherController oc)
    {
        // ValidateAction(oc.gameObject);
        if (cratesHolder.TryGetValue(packet.Token, out Crate player))
        {
            if (oc.crateList.Count == 0) // TODO: Handle doing skills better
            {
                // player.DoSkill(packet.Action);
                return true;
            }
            else
            {
                if (cratesHolder.TryGetValue(oc.crateList[oc.crateList.Count - 1], out Crate crate))
                {
                    crate.DoAction(player, packet.Action);
                    return true;
                }
            }
        }
        return false;
    }

    void UpdateCrateTimers()
    {
        foreach (var crate in cratesHolder)
        {
            // Get Rob Crate --- Add 1 Drug
            if (crate.Value.Access == AccessCode.Robs && crate.Value.Role == RoleCode.Null && crate.Value.Drugs < 100)
            {
                crate.Value.Drugs += 1;
            }
        }
    }

    void AddExportTimer(object sender, Crate crate)
    {
        if (exportHolder.TryGetValue(crate.Id, out List<float> timers))
        {
            timers.Add(0f);
        }
    }

    void RemoveExportTimer(object sender, Crate crate)
    {
        if (exportHolder.TryGetValue(crate.Id, out List<float> timers))
        {
            timers.RemoveAt(timers.Count - 1);
        }
    }


    // bool ValidateAction(GameObject gameObject)
    // {
    //     var colliders = Physics.OverlapSphere(gameObject.transform.position, 0.0f);
    //     return false;
    // }
}

public enum AccessCode
{
    Null,
    Cops,
    Robs,
}

public enum RoleCode
{
    Null,
    _1,
    _2,
    _3,
    _4,
    _5,
    _6,
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
    UseJail,
}

public class Crate
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Drugs { get; set; }
    public int Evidence { get; set; }
    public int Score { get; set; }
    public bool IsExport { get; set; }
    public RoleCode Role { get; set; }
    public AccessCode Access { get; set; }

    public event System.EventHandler<Crate> AddExportTimer;
    public event System.EventHandler<Crate> RemoveExportTimer;

    public Crate(string id, string name)
    {
        Id = id;
        Name = name;
        Role = RoleCode.Null;
        Access = AccessCode.Null;
        IsExport = false;
        Evidence = 0;
        Drugs = 0;
    }

    public bool CanRole1View(Crate player)
    {
        return Access == player.Access && (Role == RoleCode._2 || Role == RoleCode._3);
    }

    public bool ScoreDrug(float timer)
    {
        if (Drugs > 0 && timer > 180f)
        {
            Drugs -= 1;
            Score += 1;
            return true;
        }
        return false;
    }

    public bool DoAction(Crate player, ActionType action)
    {
        // Crate has Drugs --- Player has no Drugs
        if (action == ActionType.GetDrugs && Drugs > 0 && player.Drugs == 0 && (player.Access == AccessCode.Robs || player.Role == RoleCode._3 || player.Role == RoleCode._4))
        {
            Drugs -= 1;
            player.Drugs += 1;
            if (IsExport) RemoveExportTimer?.Invoke(this, this);
            return true;
        }

        // Crate can hold more Drugs --- Player has Drugs
        if (action == ActionType.StoreDrugs && Drugs < 100 && player.Drugs > 0 && (player.Access == AccessCode.Robs || player.Role == RoleCode._3 || player.Role == RoleCode._4))
        {
            Drugs += 1;
            player.Drugs -= 1;

            if ((player.Access == AccessCode.Robs && player.Role == RoleCode._1) || // Robert
            (player.Access == AccessCode.Robs && player.Role == RoleCode._2) ||     // Criminal
            (player.Access == AccessCode.Cops && player.Role == RoleCode._3) ||     // Mob Cop
            (player.Access == AccessCode.Cops && player.Role == RoleCode._4) ||     // Crooked Cop
            (player.Access == AccessCode.Robs && player.Role == RoleCode._5))       // Street Thug
                Evidence += 1; // Evidence will be created

            if (Access == AccessCode.Robs) Evidence = 0; // Rob Crate has no Evidence
            if (Access == AccessCode.Null && Evidence > 1) Evidence = 1; // Normal Crate max 1 Evidence

            if (IsExport) AddExportTimer?.Invoke(this, this);
            return true;
        }

        // Crate has Evidence --- Player has no Evidence
        if (action == ActionType.GetEvidence && Evidence > 0 && player.Evidence == 0 && (player.Access == AccessCode.Cops || player.Role == RoleCode._3 || player.Role == RoleCode._4))
        {
            Evidence -= 1;
            player.Evidence += 1;
            return true;
        }

        // Crate can hold more Evidence --- Player has Evidence
        if (action == ActionType.StoreEvidence && Evidence < 100 && player.Evidence > 0 && (player.Access == AccessCode.Cops || player.Role == RoleCode._3 || player.Role == RoleCode._4))
        {
            if (Access == AccessCode.Null && Evidence > 0) return false; // Normal Crate max 1 Evidence --- CANCEL ACTION
            if (Access == AccessCode.Robs) return false; // Rob Crate can't hold Evidence --- CANCEL ACTION
            Evidence += 1;
            player.Evidence -= 1;
            return true;
        }

        // Crate can hold more Evidence --- Player has Evidence
        if (action == ActionType.CreateWarrant && Evidence >= 7 && (player.Access == AccessCode.Cops || player.Role == RoleCode._3 || player.Role == RoleCode._4))
        {
            if (Access == AccessCode.Null && Evidence > 0) return false; // Normal Crate max 1 Evidence --- CANCEL ACTION
            if (Access == AccessCode.Robs) return false; // Rob Crate can't hold Evidence --- CANCEL ACTION
            Evidence += 1;
            player.Evidence -= 1;
            return true;
        }

        return false;
    }

    public bool DoSkill(ActionType action)
    {
        // Player has no Evidence --- Player can create Evidence
        if (action == ActionType.CreateEvidence && Evidence == 0 && Access == AccessCode.Robs && Role == RoleCode._3)
        {
            Evidence += 1;
            return true;
        }

        // Player has Evidence --- Player can destroy Evidence
        if (action == ActionType.DestroyEvidence && Evidence > 0 && Access == AccessCode.Cops && Role == RoleCode._3)
        {
            Evidence -= 1;
            return true;
        }

        return false;
    }
}
