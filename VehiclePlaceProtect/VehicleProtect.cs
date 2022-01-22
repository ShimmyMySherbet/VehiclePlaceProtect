using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace VehiclePlaceProtect
{
    public class VehicleProtect : RocketPlugin
    {
        public override void LoadPlugin()
        {
            base.LoadPlugin();
            BarricadeManager.onDeployBarricadeRequested += OnDeployRequested;
            Logger.Log("Vehicle Place Protect by ShimmyMySherbet loaded.");
            Logger.Log("Prevents players placing objects on other player's vehicles");
        }

        public override void UnloadPlugin(PluginState state = PluginState.Unloaded)
        {
            BarricadeManager.onDeployBarricadeRequested -= OnDeployRequested;
            base.UnloadPlugin(state);
        }

        private void OnDeployRequested(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!shouldAllow || hit == null || owner == 0)
                return;

            var vehicle = hit.GetComponent<InteractableVehicle>();
            var player = PlayerTool.getPlayer(new CSteamID(owner));
            if (vehicle == null || player == null)
                return;

            var sender = UnturnedPlayer.FromPlayer(player);

            if (!vehicle.isLocked || vehicle.lockedOwner.m_SteamID == 0)
            {
                shouldAllow = false;
                if (sender != null)
                {
                    UnturnedChat.Say(sender, Translate("Vehicle_DenyPlace_NotLocked"), Color.red, true);
                }
                return;
            }

            if (vehicle.lockedOwner.m_SteamID != owner &&
                (vehicle.lockedGroup == CSteamID.Nil || vehicle.lockedGroup.m_SteamID != group))
            {
                shouldAllow = false;
                if (sender != null)
                {
                    UnturnedChat.Say(sender, Translate("Vehicle_DenyPlace_NotLocked"), Color.red, true);
                }
                return;
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "Vehicle_DenyPlace_NotLocked", "The vehicle must be locked to place buildables on it." },
            { "Vehicle_DenyPlace_OtherPlayers", "You cannot place buildables on other player's vehicles."}
        };
    }
}