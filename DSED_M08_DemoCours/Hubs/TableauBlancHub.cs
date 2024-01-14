using DSED_M08_DemoCours.Entite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace DSED_M08_DemoCours.Hubs
{
    public class TableauBlancHub : Hub
    {
        public static int TotalViews { get; set; } = 0;
        public static Dictionary<string, string> m_Tableaux = new Dictionary<string, string>();
        public static Dictionary<string, List<Ligne>> _dessin = new Dictionary<string, List<Ligne>>();

        public async Task DemarrerTableau(string nomTableau)
        {
            string connexionId = this.Context.ConnectionId;
            if (!m_Tableaux.ContainsKey(connexionId))
            {
                m_Tableaux.Add(connexionId, nomTableau);
                _dessin.Add(nomTableau, new List<Ligne>());
                await Clients.Caller.SendAsync("DemarrageTableau", _dessin[nomTableau]);
            }

            if (!_dessin.ContainsKey(nomTableau))
            {
                _dessin.Add(nomTableau, new List<Ligne>());
                await Clients.All.SendAsync("MetterAJour", _dessin.Keys);
            }

            await Groups.AddToGroupAsync(connexionId, nomTableau);
            m_Tableaux[connexionId] = nomTableau;
            await Clients.Caller.SendAsync("DemarrageTableau", _dessin.Keys);
        }

        public async Task UpdateSessionName()
        {
            string connexionId = this.Context.ConnectionId;
            if (m_Tableaux.ContainsKey(connexionId))
            {
                string nomTableau = m_Tableaux[connexionId];
                await Clients.Caller.SendAsync("UpdateSessionName", nomTableau);
            }
        }

        public async Task CountViewTest() 
        {
            TotalViews++;
            await Clients.All.SendAsync("updateTotalViews", TotalViews);
        }

        public async Task DessinerLigne(Ligne p_ligne)
        {
            string connexionId = this.Context.ConnectionId;
            if (m_Tableaux.ContainsKey(connexionId))
            {
                string nomTableau = m_Tableaux[connexionId];
                _dessin[nomTableau].Add(p_ligne);
                await Clients.All.SendAsync("DessinerLigne", p_ligne);
            }

        }

        public async Task EffacerTableau()
        {
            string connexionId = this.Context.ConnectionId;
            if (m_Tableaux.ContainsKey(connexionId))
            {
                string nomTableau = m_Tableaux[connexionId];
                _dessin[nomTableau].Clear();
                await Clients.All.SendAsync("EffacerTableau");
            }
        }

        public async override Task OnConnectedAsync()
        {
            string connexionId = this.Context.ConnectionId;
            if (m_Tableaux.ContainsKey(connexionId))
            {
                string nomTableau = m_Tableaux[connexionId];
                await Clients.Caller.SendAsync("DemarrageTableau", _dessin[nomTableau]);
                await base.OnConnectedAsync();
            }
                
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }

    }
}
