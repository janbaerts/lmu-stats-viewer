using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace LmuStatsViewer.Domain;

public class DrivingSession
{
    public string SessionFile { get; set; } = string.Empty;
    
    public string TrackCourse { get; set; } = string.Empty;
    public string TrackEvent { get; set; } = string.Empty;
    public string TrackVenue { get; set; } = string.Empty;
    public double TrackLengthInMeters { get; set; }
    
    public double BestLapTime { get; set; }
    public string CarClass { get; set; } = string.Empty;
    public string CarType { get; set; } = string.Empty;
    
    public SessionType SessionType { get; set; } = SessionType.Unknown;
    public int CompletedLaps { get; set; }

    public double DrivenDistanceInMeters => TrackLengthInMeters * CompletedLaps;
    
    private static XmlSerializer serializer = new XmlSerializer(typeof(RFactorXml));

    public static DrivingSession MakeFromFile(string filePath)
    {
        DrivingSession session = new DrivingSession();

        if (File.Exists(filePath))
        {
            try
            {
                var plainXmlForCleanup = File.ReadAllText(filePath);
                plainXmlForCleanup = plainXmlForCleanup.Replace("\uFFFD", "");
        
                using var reader = new StringReader(plainXmlForCleanup);
                var result = (RFactorXml)serializer.Deserialize(reader);
                Console.WriteLine(Path.GetFileName(filePath));

                if (result == null) return session;

                session.SessionFile = Path.GetFileName(filePath);

                session.TrackCourse = result.RaceResults.TrackCourse;
                session.TrackEvent = result.RaceResults.TrackEvent;
                session.TrackVenue = result.RaceResults.TrackVenue;
                session.TrackLengthInMeters = result.RaceResults.TrackLength;

                XmlDrivingSession? xmlSession = null;
                XmlDrivingSession[] allXmlSessions =
                [
                    result.RaceResults.Practice1, result.RaceResults.Practice2, result.RaceResults.Practice3,
                    result.RaceResults.Practice4, result.RaceResults.Qualify, result.RaceResults.Race
                ];

                for (int i = 0; i < allXmlSessions.Length; i++)
                {
                    if (allXmlSessions[i] != null)
                    {
                        xmlSession = allXmlSessions[i];
                        if (i <= 3) session.SessionType = SessionType.Practice;
                        else if (i == 4) session.SessionType = SessionType.Qualify;
                        else session.SessionType = SessionType.Race;
                    }
                }

                if (xmlSession != null)
                {
                    Driver? player = xmlSession.Drivers.Find(x => x.Name == Settings.PlayerName);
                    if (player != null)
                    {
                        session.CompletedLaps = EstimateCompletedLaps(player);
                        session.BestLapTime = player.BestLapTime;
                        session.CarClass = player.CarClass;
                        session.CarType = player.CarType;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file: " + Path.GetFileName(filePath));
                Console.WriteLine(e.Message);
            }
            // TODO (JB) You are here.
        }

        return session;
    }

    private static int EstimateCompletedLaps(Driver driver)
    {
        // (JB) With the current data available, it's not possible to calculate driven distance accurately. Laps with
        //      track limits warning that discount the lap, don't have a lap time, but so do laps that you did not complete
        //      but returned to garage for example. So I make an estimate: all the laps in the collection and I deduct
        //      one whole lap.
        
        return driver.Laps.Count - 1;
    }
}