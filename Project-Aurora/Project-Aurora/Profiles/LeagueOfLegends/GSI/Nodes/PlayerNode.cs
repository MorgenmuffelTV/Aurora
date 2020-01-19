﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public enum Champion
    {
        Undefined = -1,
        Aatrox,
        Ahri,
        Akali,
        Alistar,
        Amumu,
        Anivia,
        Annie,
        Aphelios,
        Ashe,
        AurelionSol,
        Azir,
        Bard,
        Blitzcrank,
        Brand,
        Braum,
        Caitlyn,
        Camille,
        Cassiopeia,
        Chogath,
        Corki,
        Darius,
        Diana,
        Draven,
        DrMundo,
        Ekko,
        Elise,
        Evelynn,
        Ezreal,
        Fiddlesticks,
        Fiora,
        Fizz,
        Galio,
        Gangplank,
        Garen,
        Gnar,
        Gragas,
        Graves,
        Hecarim,
        Heimerdinger,
        Illaoi,
        Irelia,
        Ivern,
        Janna,
        JarvanIV,
        Jax,
        Jayce,
        Jhin,
        Jinx,
        Kaisa,
        Kalista,
        Karma,
        Karthus,
        Kassadin,
        Katarina,
        Kayle,
        Kayn,
        Kennen,
        Khazix,
        Kindred,
        Kled,
        KogMaw,
        Leblanc,
        LeeSin,
        Leona,
        Lissandra,
        Lucian,
        Lulu,
        Lux,
        Malphite,
        Malzahar,
        Maokai,
        MasterYi,
        MissFortune,
        MonkeyKing,
        Mordekaiser,
        Morgana,
        Nami,
        Nasus,
        Nautilus,
        Neeko,
        Nidalee,
        Nocturne,
        Nunu,
        Olaf,
        Orianna,
        Ornn,
        Pantheon,
        Poppy,
        Pyke,
        Qiyana,
        Quinn,
        Rakan,
        Rammus,
        RekSai,
        Renekton,
        Rengar,
        Riven,
        Rumble,
        Ryze,
        Sejuani,
        Senna,
        Sett,
        Shaco,
        Shen,
        Shyvana,
        Singed,
        Sion,
        Sivir,
        Skarner,
        Sona,
        Soraka,
        Swain,
        Sylas,
        Syndra,
        TahmKench,
        Taliyah,
        Talon,
        Taric,
        Teemo,
        Thresh,
        Tristana,
        Trundle,
        Tryndamere,
        TwistedFate,
        Twitch,
        Udyr,
        Urgot,
        Varus,
        Vayne,
        Veigar,
        Velkoz,
        Vi,
        Viktor,
        Vladimir,
        Volibear,
        Warwick,
        Xayah,
        Xerath,
        XinZhao,
        Yasuo,
        Yorick,
        Yuumi,
        Zac,
        Zed,
        Ziggs,
        Zilean,
        Zoe,
        Zyra
    }

    public enum Team
    {
        Undefined = -1,
        Order,
        Chaos
    }

    public enum SummonerSpell
    {
        Undefined = -1,
        Exhaust,
        Flash,
        Ghost,
        Heal,
        Smite,
        Teleport,
        Clarity,
        Ignite,
        Barrier,
        Mark
    }

    public class PlayerNode : Node<PlayerNode>
    {
        public StatsNode ChampionStats = new StatsNode();
        public int Level;
        public float Gold;
        public string SummonerName;
        public bool IsDead;
        public Champion Champion;
        public int Kills;
        public int Deaths;
        public int Assists;
        public int CreepScore;
        public float WardScore;
        public float RespawnTimer;
        public Team Team;
        public SummonerSpell SpellD;
        public SummonerSpell SpellF;
    }
}
