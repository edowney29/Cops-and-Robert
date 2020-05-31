﻿using Dissonance;
using Dissonance.Networking;
using System;

public class CustomCommsNetwork : BaseCommsNetwork<CustomServer, CustomClient, CustomConn, Unit, Unit>
{
    NetworkManager networkManager;

    protected override void Initialize()
    {
        networkManager = GetComponent<NetworkManager>();

        // Don't forget to call base.Initialize!   
        base.Initialize();
    }

    // Check every frame
    protected override void Update()
    {
        if (IsInitialized)
        {

            bool client = true;
            bool server = networkManager.IsServer;
            // Check what mode Dissonance is in and if they're different then call the correct method
            if (Mode.IsServerEnabled() != server || Mode.IsClientEnabled() != client)
            {
                // HLAPI is server and client, so run as a non-dedicated
                // host (passing in the correct parameters)
                if (server)
                    RunAsHost(Unit.None, Unit.None);

                // HLAPI is just a server, so run as a dedicated host
                // else if (server)
                //     RunAsDedicatedServer(Unit.None);

                // HLAPI is just a client, so run as a client
                else //if (client)
                    RunAsClient(Unit.None);
            }
        }
        else if (Mode != NetworkMode.None)
        {
            //Network is not active, make sure Dissonance is not active
            Stop();
        }

        base.Update();
    }

    // We specified `Unit` as `TServerParam`, so we get given a `Unit`
    protected override CustomServer CreateServer(Unit details)
    {
        return new CustomServer(networkManager);
    }

    // We specified `Unit` as `TClientParam`, so we get given a `Unit`
    protected override CustomClient CreateClient(Unit details)
    {
        return new CustomClient(this, networkManager);
    }
}

public struct CustomConn : IEquatable<CustomConn>
{
    public string token;

    public bool Equals(CustomConn other)
    {
        return this.token.Equals(other.token);
    }
}