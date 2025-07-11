﻿using System.Xml.Serialization;

namespace LmuStatsViewer.Domain;

[XmlRoot("rFactorXML")]
public class RFactorXml
{
    [XmlElement("RaceResults")]
    public RaceResults RaceResults { get; set; }
}

public class RaceResults
{
    public string TimeString { get; set; }
    public string TrackVenue { get; set; }
    public string TrackCourse { get; set; }
    public string TrackEvent { get; set; }
    public double TrackLength { get; set; }
    public int RaceLaps { get; set; }
    public int RaceTime { get; set; }
    public int MechFailRate { get; set; }
    public int DamageMult { get; set; }
    public int FuelMult { get; set; }
    public int TireMult { get; set; }
    public int ParcFerme { get; set; }
    public int FixedSetups { get; set; }
    public int FreeSettings { get; set; }
    public int FixedUpgrades { get; set; }
    public int LimitedTires { get; set; }
    public int TireWarmers { get; set; }

    [XmlElement("Qualify")]
    public XmlDrivingSession Qualify { get; set; }

    [XmlElement("Race")]
    public XmlDrivingSession Race { get; set; }
    
    [XmlElement("Practice1")]
    public XmlDrivingSession Practice1 { get; set; }
    [XmlElement("Practice2")]
    public XmlDrivingSession Practice2 { get; set; }
    [XmlElement("Practice3")]
    public XmlDrivingSession Practice3 { get; set; }
    [XmlElement("Practice4")]
    public XmlDrivingSession Practice4 { get; set; }

    [XmlElement("Driver")]
    public List<Driver> Drivers { get; set; }
}

public class ConnectionType
{
    [XmlAttribute("upload")]
    public int Upload { get; set; }

    [XmlAttribute("download")]
    public int Download { get; set; }

    [XmlText]
    public string Type { get; set; }
}

public class DrivingStream
{
    [XmlElement("Score", typeof(Score))]
    [XmlElement("Penalty", typeof(Penalty))]
    [XmlElement("Incident", typeof(Incident))]
    [XmlElement("Sector", typeof(Sector))]
    [XmlElement("Chat", typeof(Chat))]
    public List<StreamEvent> Events { get; set; }
}


public abstract class StreamEvent
{
    [XmlAttribute("et")]
    public float ElapsedTime { get; set; }

    [XmlText]
    public string Value { get; set; }
}

public class Score : StreamEvent { }
public class Penalty : StreamEvent { }
public class Incident : StreamEvent { }
public class Sector : StreamEvent { }
public class Chat : StreamEvent { }

public class XmlDrivingSession
{
    public long DateTime { get; set; }
    public string TimeString { get; set; } = string.Empty;
    public int Laps { get; set; }
    public int Minutes { get; set; }

    [XmlElement("Stream")]
    public List<DrivingStream> Stream { get; set; }
    
    [XmlElement("Driver")]
    public List<Driver> Drivers { get; set; }
}

public class SentMessage
{
    [XmlAttribute("et")]
    public float ElapsedTime { get; set; }

    [XmlText]
    public string Message { get; set; }
}

public class Driver
{
    public string Name { get; set; }
    public int Connected { get; set; }
    public string VehFile { get; set; }
    public string UpgradeCode { get; set; }
    public string VehName { get; set; }
    public string Category { get; set; }
    public string CarType { get; set; }
    public string CarClass { get; set; }
    public int CarNumber { get; set; }
    public string TeamName { get; set; }
    public int isPlayer { get; set; }
    public int ServerScored { get; set; }
    public int Position { get; set; }
    public int ClassPosition { get; set; }

    [XmlElement("Lap")]
    public List<Lap> Laps { get; set; }

    public double BestLapTime { get; set; }
    public int Pitstops { get; set; }
    public string FinishStatus { get; set; }
    public string ControlAndAids { get; set; }
}

public class Lap
{
    [XmlAttribute("num")]
    public int Number { get; set; }

    [XmlAttribute("p")]
    public int Position { get; set; }

    [XmlAttribute("et")]
    public string ElapsedTime { get; set; }

    [XmlAttribute("s1")] 
    public double Sector1 { get; set; }
    
    [XmlAttribute("s2")]
    public double Sector2 { get; set; }
    
    [XmlAttribute("s3")]
    public double Sector3 { get; set; }
    
    [XmlAttribute("topspeed")]
    public double TopSpeed { get; set; }
    
    [XmlAttribute("fuel")]
    public double Fuel { get; set; }

    [XmlAttribute("fuelUsed")]
    public double FuelUsed { get; set; }

    [XmlText]
    public string LapTime { get; set; }
}
