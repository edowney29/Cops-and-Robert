using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    float gameTimer = 0f, tickTimer = 0f;
    protected bool isRunning = false;

    Dictionary<string, Crate> cratesHolder = new Dictionary<string, Crate>();

    void Update()
    {
        var time = Time.deltaTime;
        if (isRunning)
        {
            gameTimer += time;
            // tickTimer += Time.deltaTime;
            foreach (var crate in cratesHolder.Values)
            {
                if (crate.IsExport) crate.UpdateTimers(time);
            }
        }
    }

    protected Dictionary<string, Crate> GetCratesHolder()
    {
        return cratesHolder;
    }

    protected void ResetGameState()
    {
        isRunning = false;
        CancelInvoke("UpdateCrateTimers");
    }

    protected void SetupGameState(Dictionary<string, OtherController> players, string token)
    {
        gameTimer = 0f;
        // tickTimer = 0f;
        var names = new PlayerNames().Names;

        cratesHolder.Clear();
        Crate _crate = new Crate();
        _crate.Id = token;
        _crate.Display = names[0];
        cratesHolder.Add(token, _crate);

        List<string> playerList = new List<string>();
        playerList.Add(token);

        int index = 0;
        foreach (var player in players)
        {
            Crate crate = new Crate();
            crate.Id = player.Key;
            crate.Display = names[++index];
            cratesHolder.Add(player.Key, crate);
            playerList.Add(player.Key);
        }

        for (int i = 0; i < playerList.Count; ++i)
        {
            string tmp = playerList[i];
            int rng = Random.Range(i, playerList.Count);
            playerList[i] = playerList[rng];
            playerList[rng] = tmp;
        }

        float rolePercent = Random.value;
        for (int i = 0; i < playerList.Count; ++i)
        {
            if (cratesHolder.TryGetValue(playerList[i], out Crate player))
            {
                if (i % 2 == 0) player.Access = AccessCode.Robs;
                else player.Access = AccessCode.Cops;

                if (i == 0 || i == 1) player.Role = RoleCode._1;
                else if (i == 2 || i == 3) player.Role = RoleCode._2;
                // else if (i == 4 || i == 5) player.Role = RoleCode._3;
                // else if ((i == 6 || i == 7) && playerList.Count >= 8) player.Role = RoleCode._4;
                // else if ((i == 8 || i == 9) && playerList.Count >= 10) player.Role = rolePercent > 0.5 ? RoleCode._5 : RoleCode._6;
                else player.Role = RoleCode._2;
            }
        }

        SetupCreates();
        InvokeRepeating("UpdateCrateTimers", 0f, 60f);
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
        int index = 0;
        foreach (var _crate in crates)
        {
            // var cc = _crate.GetComponent<CrateController>();
            Crate crate = new Crate();
            crate.Id = System.Guid.NewGuid().ToString();
            crate.Display = "Crate";
            crate.Access = AccessCode.Null;
            crate.UpdateTransform(_crate.transform);
            if (index == 0)
            {
                crate.IsExport = true;
                // crate.AddExportTimer += AddExportTimer;
                // crate.RemoveExportTimer += RemoveExportTimer;
                crate.Display = "Exports";
            }
            cratesHolder.Add(crate.Id, crate);
            // cc.SetCrate(crate);
            _crate.SetActive(false);
            index += 1;
        }

        var rcrates = GameObject.FindGameObjectsWithTag("RobCrate");
        foreach (var _crate in rcrates)
        {
            // var cc = _crate.GetComponent<CrateController>();
            Crate crate = new Crate();
            crate.Id = System.Guid.NewGuid().ToString();
            crate.Display = "Drug Stash";
            crate.Access = AccessCode.Robs;
            crate.UpdateTransform(_crate.transform);
            cratesHolder.Add(crate.Id, crate);
            _crate.SetActive(false);
        }

        var ccrates = GameObject.FindGameObjectsWithTag("CopCrate");
        foreach (var _crate in ccrates)
        {
            // var cc = _crate.GetComponent<CrateController>();
            Crate crate = new Crate();
            crate.Id = System.Guid.NewGuid().ToString();
            crate.Display = "Police Locker";
            crate.Access = AccessCode.Cops;
            crate.UpdateTransform(_crate.transform);
            cratesHolder.Add(crate.Id, crate);
            _crate.SetActive(false);
        }
    }

    protected bool UpdateGameState(PlayerPacket packet, OtherController oc)
    {
        if (cratesHolder.TryGetValue(packet.Token, out Crate player) && cratesHolder.TryGetValue(packet.ActionCrate, out Crate crate)) // && oc.crateList.Exists(x => x.Equals(packet.ActionCrate)))
        {
            return crate.DoAction(player, packet.Action);
        }
        return false;
    }

    protected bool UpdateGameStateServer(string token, string id, ActionType action)
    {
        if (cratesHolder.TryGetValue(token, out Crate player) && cratesHolder.TryGetValue(id, out Crate crate))
        {
            return crate.DoAction(player, action);
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

    }

    void RemoveExportTimer(object sender, Crate crate)
    {

    }
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
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public float RotX { get; set; }
    public float RotY { get; set; }
    public float RotZ { get; set; }
    public string Id { get; set; }
    public string Display { get; set; }
    public int Drugs { get; set; }
    public int Evidence { get; set; }
    public int Score { get; set; }
    public bool IsExport { get; set; }
    public RoleCode Role { get; set; }
    public AccessCode Access { get; set; }
    List<float> Timers = new List<float>();

    // public event System.EventHandler<Crate> AddExportTimer;
    // public event System.EventHandler<Crate> RemoveExportTimer;

    public void UpdateTransform(Transform transform)
    {
        PosX = transform.position.x;
        PosY = transform.position.y;
        PosZ = transform.position.z;
        RotX = transform.rotation.eulerAngles.x;
        RotY = transform.rotation.eulerAngles.y;
        RotZ = transform.rotation.eulerAngles.z;
    }

    public void UpdateTimers(float addTime)
    {
        for (int i = 0; i < Timers.Count; ++i)
        {
            Timers[i] += addTime;
        }
        Timers.RemoveAll(timer => ScoreDrug(timer));
    }

    public bool CanRole1View(Crate player)
    {
        return Access == player.Access && (Role == RoleCode._2 || Role == RoleCode._3);
    }

    public bool ScoreDrug(float timer)
    {
        if (Drugs > 0 && timer > 60f)
        {
            Drugs -= 1;
            Score += 1;
            return true;
        }
        return false;
    }

    public bool DoAction(Crate player, ActionType action)
    {
        Debug.Log("[ACTION]: " + player.Access + " - " + player.Role + " - " + action);
        // Crate has Drugs --- Player has no Drugs
        if (action == ActionType.GetDrugs && Drugs > 0 && player.Drugs == 0 && (player.Access == AccessCode.Robs || player.Role == RoleCode._3 || player.Role == RoleCode._4))
        {
            Drugs -= 1;
            player.Drugs += 1;
            // if (IsExport) RemoveExportTimer?.Invoke(this, this);
            if (IsExport) Timers.RemoveAt(Timers.Count - 1);
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

            // if (IsExport) AddExportTimer?.Invoke(this, this);
            if (IsExport) Timers.Add(0f);
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
