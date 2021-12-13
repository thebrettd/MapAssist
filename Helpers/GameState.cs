using MapAssist.Helpers;
using MapAssist.Settings;
using System;
using System.Collections.Generic;

namespace MapAssist.Types
{
    public abstract class BaseGameState
    {
        public string GameName;
        public string GamePass;
        public string PlayerName;
    }
    
    public class GameState : BaseGameState
    {
        public static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        // Store the current state including current game data
        // and previous game state
        public IntPtr MainWindowHandle;
        public GameData GameData;
        private MapApi MapApi;
        public long Frames;
        
        // A trimmed down version of this state
        public LastGameState LastGameState;

        public GameState(GameData gameData, GameState lastGameState, IntPtr mainWindowHandle)
        {
            GameData = gameData;
            MainWindowHandle = mainWindowHandle;
            
            if (GameData != null)
            {
                MapApi = HasGameChanged(lastGameState) ? new MapApi(gameData.Difficulty, gameData.MapSeed) : lastGameState.MapApi;

                var mapChanged = HasMapChanged(lastGameState);
                if (HasMapChanged(lastGameState))
                {
                    if (GameData.Area != Area.None)
                    {
                        _log.Info($"Area changed: {GameData.Area}"); 
                        GameData.AreaData = MapApi.GetMapData(GameData.Area);
                        GameData.AreaData.CalcViewAreas(Compositor.RotateRadians);

                        var pointsOfInterest = new List<PointOfInterest>();

                        if (GameData.AreaData != null)
                        {
                            pointsOfInterest = PointOfInterestHandler.Get(MapApi, GameData.AreaData);
                        }

                        GameData.PointOfInterests = pointsOfInterest;
                    }
                }
                else
                {
                    GameData.AreaData = lastGameState.GameData.AreaData;
                    GameData.PointOfInterests = lastGameState.GameData.PointOfInterests;
                }
            }

            if (lastGameState != null)
            {
                LastGameState = lastGameState.ToLastGameState();
                Frames = lastGameState.Frames;
            }
            else
            {
                Frames = 0;
            }
        }
        private LastGameState ToLastGameState()
        {
            return LastGameState.FromGameState(this);
        }
        
        private bool HasGameChanged(GameState other)
        {
            if (other == null) return true;
            if (GameData == null) return true;
            if (GameData.HasChanged(other.GameData)) return true;
            return PlayerName != other.PlayerName;
        }
       
        private bool HasMapChanged(GameState other)
        {
            if (other == null) return true;
            if (GameData?.Area == null || other.GameData?.Area == null) return true;
            return HasGameChanged(other) || GameData.Area != other.GameData.Area;
        }
    }

    public class LastGameState : BaseGameState
    {
        public static LastGameState FromGameState(GameState gameState)
        {
            return new LastGameState() {GameName = gameState.GameName, GamePass = gameState.GamePass};
        }
    }
}
