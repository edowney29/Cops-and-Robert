using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject cratesObject;
    float gameTimer = 0f;
    protected bool serverRunning = false;
    Dictionary<string, Crate> cratesHolder = new Dictionary<string, Crate>();

    void Update()
    {
        var timer = Time.deltaTime;
        if (serverRunning)
        {
            gameTimer += timer;
            bool isOvertime = gameTimer > 1200f;
            foreach (var crate in cratesHolder.Values)
            {
                crate.UpdateTimers(timer, gameTimer, isOvertime);
            }
        }
    }

    protected Dictionary<string, Crate> GetCratesHolder()
    {
        return cratesHolder;
    }

    protected void ResetGameState()
    {
        serverRunning = false;
        foreach (var crate in GameObject.FindGameObjectsWithTag("RobCrate"))
        {
            crate.SetActive(false);
            // Destroy(crate.gameObject);
        }
        foreach (var crate in GameObject.FindGameObjectsWithTag("CopCrate"))
        {
            crate.SetActive(false);
            // Destroy(crate.gameObject);
        }
        foreach (var crate in GameObject.FindGameObjectsWithTag("Crate"))
        {
            crate.SetActive(false);
            // Destroy(crate.gameObject);
        }
        StopCoroutine("UpdateCrateTimers");
    }

    protected void SetupGameState(Dictionary<string, OtherController> players, string token)
    {
        cratesHolder.Clear();
        // string[] displayNames = new PlayerNames().Names;

        Crate _crate = new Crate();
        _crate.Id = token;
        // _crate.Display = displayNames[0];
        cratesHolder.Add(token, _crate);

        List<string> playerList = new List<string>();
        playerList.Add(token);

        // int index = 0;
        foreach (var player in players)
        {
            if (player.Value.isActiveAndEnabled)
            {
                Crate crate = new Crate();
                crate.Id = player.Key;
                // crate.Display = displayNames[++index];
                cratesHolder.Add(player.Key, crate);
                playerList.Add(player.Key);
            }
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
                else if (i == 4 || i == 5) player.Role = RoleCode._3;
                else if ((i == 6 || i == 7) && playerList.Count >= 8) player.Role = RoleCode._4;
                else if ((i == 8 || i == 9) && playerList.Count >= 10) player.Role = rolePercent > 0.5 ? RoleCode._5 : RoleCode._6;
                else player.Role = RoleCode._2;

                // if (player.Access == AccessCode.Cops && player.Role == RoleCode._3) player.UpdateCooldown(ActionType.DestroyEvidence);
                // if (player.Access == AccessCode.Robs && player.Role == RoleCode._3) player.UpdateCooldown(ActionType.CreateEvidence);
                // if (player.Access == AccessCode.Robs) player.UpdateCooldown(ActionType.InJail, 0f);
            }
        }

        SetupCreates();
        gameTimer = 0f;
        serverRunning = true;
    }

    void SetupCreates()
    {
        for (int i = 0; i < cratesObject.transform.childCount; ++i)
        {
            cratesObject.transform.GetChild(i).gameObject.SetActive(true);
        }

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
            Crate crate = new Crate();
            crate.Id = System.Guid.NewGuid().ToString();
            crate.Display = "Crate";
            crate.Access = AccessCode.Null;
            crate.UpdateTransform(_crate.transform);
            if (index == 0)
            {
                crate.IsExport = true;
                crate.Display = "Exports";
            }
            cratesHolder.Add(crate.Id, crate);
            _crate.SetActive(false);
            index += 1;
        }

        var rcrates = GameObject.FindGameObjectsWithTag("RobCrate");
        foreach (var _crate in rcrates)
        {
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
            Crate crate = new Crate();
            crate.Id = System.Guid.NewGuid().ToString();
            crate.Display = "Police Locker";
            crate.Access = AccessCode.Cops;
            crate.UpdateTransform(_crate.transform);
            cratesHolder.Add(crate.Id, crate);
            _crate.SetActive(false);
        }

        StartCoroutine("UpdateCrateTimers");
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

    System.Collections.IEnumerator UpdateCrateTimers()
    {
        foreach (var crate in cratesHolder)
        {
            // Get Rob Crate --- Add 1 Drug
            if (crate.Value.Access == AccessCode.Robs && crate.Value.Role == RoleCode.Null && crate.Value.Drugs < 100)
            {
                crate.Value.Drugs += 1;
            }
        }
        yield return new WaitForSeconds(gameTimer < TimerValues.Overtime ? TimerValues.ExportTime : TimerValues.ExportTime / 2f);
    }

    protected ActionType DetermineAction(Crate myCrate, InputType input)
    {
        // if (input == InputType.CreateEvidence) return ActionType.CreateEvidence;
        // if (input == InputType.DestroyEvidence) return ActionType.DestroyEvidence;
        if (input == InputType.CreateWarrant) return ActionType.CreateWarrant;
        if (input == InputType.UseWarrant) return ActionType.UseWarrant;

        if (input == InputType.F)
        {
            if (myCrate.Drugs > 0) return ActionType.StoreDrugs;
            if (myCrate.Drugs == 0) return ActionType.GetDrugs;
        }

        if (input == InputType.R)
        {
            if (myCrate.Evidence > 0) return ActionType.StoreEvidence;
            if (myCrate.Evidence == 0) return ActionType.GetEvidence;
        }

        if (input == InputType.C)
        {
            if (myCrate.Warrants > 0) return ActionType.StoreWarrant;
            if (myCrate.Warrants == 0) return ActionType.GetWarrant;
        }

        return ActionType.Null;
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
    GetWarrant,
    StoreWarrant,
    CreateWarrant,
    UseWarrant,
    UseJail4,
    UseJail5,
    UseJail6,
    InJail
}

public enum InputType
{
    NULL,
    R,
    F,
    C,
    CreateWarrant,
    UseWarrant,
    // CreateEvidence,
    // DestroyEvidence
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
    public int Warrants { get; set; }
    public int Score { get; set; }
    public bool IsExport { get; set; }
    public bool IsOvertime { get; set; }
    public RoleCode Role { get; set; }
    public AccessCode Access { get; set; }
    List<float> ExportTimers = new List<float>();
    Dictionary<ActionType, float> CooldownTimers = new Dictionary<ActionType, float>();

    public void UpdateTransform(Transform transform)
    {
        PosX = transform.position.x;
        PosY = transform.position.y;
        PosZ = transform.position.z;
        RotX = transform.rotation.eulerAngles.x;
        RotY = transform.rotation.eulerAngles.y;
        RotZ = transform.rotation.eulerAngles.z;
    }

    public void UpdateTimers(float addTime, float gameTimer, bool isOvertime)
    {
        IsOvertime = isOvertime;
        foreach (var key in CooldownTimers.Keys)
        {
            CooldownTimers[key] -= addTime;
        }
        for (int i = 0; i < ExportTimers.Count; ++i)
        {
            ExportTimers[i] += addTime;
        }
        ExportTimers.RemoveAll(timer => ScoreDrug(timer));
    }

    // public void UpdateCooldown(ActionType action, float time = 0f)
    // {
    //     CooldownTimers.Add(action, time == 0f ? TimerValues.CooldownTime(action) : time);
    // }

    // public bool CanRole1View(Crate player)
    // {
    //     return Access == player.Access && (Role == RoleCode._2 || Role == RoleCode._3);
    // }

    public bool ScoreDrug(float timer)
    {
        if (Drugs > 0 && timer > TimerValues.ExportTime)
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
            if (IsExport) ExportTimers.RemoveAt(ExportTimers.Count - 1);
            return true;
        }

        // Crate can hold more Drugs --- Player has Drugs --- Any Role
        if (action == ActionType.StoreDrugs && Drugs < 100 && player.Drugs > 0)
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

            if (IsExport) ExportTimers.Add(0f);
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

        // Crate has Warrants --- Player has no Warrant --- Crate is Cops
        if (action == ActionType.GetWarrant && Warrants > 0 && player.Warrants == 0 && player.Access == AccessCode.Cops && Access == AccessCode.Cops)
        {
            Warrants -= 1;
            player.Warrants += 1;
            return true;
        }

        // Crate can store Warrants --- Player has a Warrant --- Crate is Cops
        if (action == ActionType.StoreWarrant && Warrants < 100 && player.Warrants > 0 && player.Access == AccessCode.Cops && Access == AccessCode.Cops)
        {
            Warrants += 1;
            player.Warrants -= 1;
            return true;
        }

        // Crate can create Warrant --- Player is Role 1 --- Crate is Cops
        if (action == ActionType.CreateWarrant && Evidence >= 3 && player.Access == AccessCode.Cops && player.Role == RoleCode._1 && Access == AccessCode.Cops)
        {
            if (IsOvertime && Evidence < 5) return false;
            Evidence -= IsOvertime ? 5 : 3;
            Warrants += 1;
            return true;
        }

        // Player has a Warrant --- Crate is Normal
        if (action == ActionType.UseWarrant && player.Warrants > 0 && player.Access == AccessCode.Cops && Access == AccessCode.Null)
        {
            player.Drugs += Drugs;
            Drugs = 0;
            player.Warrants -= 1;
            return true;
        }

        return false;
    }

    public bool DoActionSelf(ActionType action)
    {
        if (CooldownTimers[action] > 0f) return false; // Cooldown not ready

        // Player has no Evidence --- Player can create Evidence
        if (action == ActionType.CreateEvidence && Evidence == 0 && Access == AccessCode.Robs && Role == RoleCode._3)
        {
            Evidence += 1;
            CooldownTimers[action] = TimerValues.CooldownTime(action);
            return true;
        }

        // Player has Evidence --- Player can destroy Evidence
        if (action == ActionType.DestroyEvidence && Evidence > 0 && Access == AccessCode.Cops && Role == RoleCode._3)
        {
            Evidence -= 1;
            CooldownTimers[action] = TimerValues.CooldownTime(action);
            return true;
        }

        return false;
    }

    public bool DoActionOther(ActionType action, Crate player, Crate otherPlayer)
    {
        // Crate can send to Jail
        if (action == ActionType.UseJail4 && Evidence >= 4 && player.Access == AccessCode.Cops)
        {
            if (Access != AccessCode.Cops) return false; // Ensure Cops crate --- CANCEL ACTION
            Evidence -= 4;
            otherPlayer.CooldownTimers[ActionType.InJail] = RollJailTime(10, TimerValues.CooldownTime(action));
            return true;
        }

        // Crate can send to Jail
        if (action == ActionType.UseJail5 && Evidence >= 5 && player.Access == AccessCode.Cops)
        {
            if (Access != AccessCode.Cops) return false; // Ensure Cops crate --- CANCEL ACTION
            Evidence -= 5;
            otherPlayer.CooldownTimers[ActionType.InJail] = RollJailTime(10, TimerValues.CooldownTime(action));
            return true;
        }

        // Crate can send to Jail
        if (action == ActionType.UseJail6 && Evidence >= 6 && player.Access == AccessCode.Cops)
        {
            if (Access != AccessCode.Cops) return false; // Ensure Cops crate --- CANCEL ACTION
            Evidence -= 6;
            otherPlayer.CooldownTimers[ActionType.InJail] = RollJailTime(10, TimerValues.CooldownTime(action));
            return true;
        }

        // Crate use Warrant --- Player has Warrant -- Cops crate is otherPlayer
        if (action == ActionType.UseWarrant && player.Warrants > 0 && otherPlayer.Access == AccessCode.Cops && otherPlayer.Role == RoleCode.Null)
        {
            var drugs = Drugs;
            Drugs = 0;
            player.Warrants -= 1;
            otherPlayer.Drugs += drugs;
            return true;
        }

        return false;
    }

    // TODO: figure out overtime
    float RollJailTime(int max, float scale, bool overTime = false)
    {
        return Random.Range(1, max) * scale;
    }

    float RollJailTimeAdvantage(int max, float scale, bool overTime = false)
    {
        float roll1 = RollJailTime(max, scale);
        float roll2 = RollJailTime(max, scale);
        return roll1 > roll2 ? roll1 : roll2;
        // var roll = roll1 > roll2 ? roll1 : roll2;
        // if (overTime) roll = roll * (overTime ? 1.5f : 1f);
    }
}