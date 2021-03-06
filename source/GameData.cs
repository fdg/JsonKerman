/*
Copyright (c) 2014 David Laurie

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace JsonKerman
{
	/**
	 * This class examines the game state and converts it to json.
	 */
	public class GameData
	{
		// TODO: Some options as to what data we want to get and what we want to ignore.
		// These can come from the query string.

		private enum BuildType
		{
			BUILD_FULL,
			BUILD_PARTIAL,
			BUILD_INDEX
		};

		public string ToJson()
		{
			// Note that System.Web.Script.* isn't fully supported on Linux yet, so we use our own builder instead.
			JsonBuilder json = new JsonBuilder();

			// If we can get hold of an instance of Game, then we can look at Game.UniversalTime.

			json.AddValue("currentScene", GetSceneName(HighLogic.LoadedScene));
			json.AddValue("isEditor", HighLogic.LoadedSceneIsEditor);
			json.AddValue("isFlight", HighLogic.LoadedSceneIsFlight);

			if (HighLogic.CurrentGame != null)
			{
				// TODO: Also check that we're not in a game and hide this.
				json.AddValue("universalTime", HighLogic.CurrentGame.UniversalTime);

				json.StartObject("game");
				BuildGame(json, HighLogic.CurrentGame);
				json.EndObject();
			}

			if (HighLogic.LoadedSceneIsFlight)
			{
				json.StartObject("flightGlobals");
				BuildFlightGlobals(json);
				json.EndObject();
			}

			return json.ToString();
		}

		private object GetSceneName(GameScenes gameScene)
		{
			switch (gameScene)
			{
				case GameScenes.LOADING:
					return "kspsplashscreen";
				case GameScenes.LOADINGBUFFER:
					return "ksploading";
				case GameScenes.MAINMENU:
					return "kspMainMenu";
				case GameScenes.SETTINGS:
					return "kspsettings";
				case GameScenes.CREDITS:
					return "credits";
				case GameScenes.SPACECENTER:
					return "spaceCenter";
				case GameScenes.EDITOR:
					return "editor";
				case GameScenes.FLIGHT:
					return "pFlight2";
				case GameScenes.TRACKSTATION:
					return "trackingStation";
				case GameScenes.PSYSTEM:
					return "PartSetupScene";
			}
			return "unknown";
		}

		private void BuildGame(JsonBuilder json, Game game)
		{
			json.AddValue("mode", game.Mode.ToString());
			json.AddValue("status", game.Status.ToString());
			json.AddValue("title", game.Title);
			json.AddValue("description", game.Description);
			json.AddValue("flagUrl", game.flagURL);
			json.AddValue("linkUrl", game.linkURL);
			json.AddValue("linkCaption", game.linkCaption);
			json.AddValue("startScene", game.startScene.ToString());

			// TODO: We can list the crew roster.

			// Not sure what context protovessels is.
		}

		private void BuildFlightGlobals(JsonBuilder json)
		{
			json.AddValue("ready", FlightGlobals.ready);
			json.AddValue("warpDriveActive", FlightGlobals.warpDriveActive);
			json.AddValue("vacuumTemperature", FlightGlobals.vacuumTemperature);

			json.StartObject("activeVessel");
			BuildVessel(json, FlightGlobals.ActiveVessel);
			json.EndObject();

			json.StartObject("otherVessels");
			foreach (Vessel vessel in FlightGlobals.Vessels)
			{
				if (vessel.isActiveVessel)
				{
					continue;
				}
				// TODO: Maybe request full data either by specifying their id on the query string, or a parameter to give full data of all vessels.
				json.StartObject(vessel.id.ToString());
				BuildVessel(json, vessel, BuildType.BUILD_PARTIAL);
				json.EndObject();
			}
			json.EndObject();

			json.StartObject("celestialBodies");
			for (int i = 0; i < FlightGlobals.Bodies.Count; i += 1) {
				json.StartObject(i.ToString());
				BuildCelestialBody(json, FlightGlobals.Bodies[i]);
				json.EndObject();
			}
			json.EndObject();
		}

		private void BuildVessel(JsonBuilder json, Vessel vessel, BuildType buildType = BuildType.BUILD_FULL)
		{
			json.AddValue("vesselName", vessel.vesselName);
			json.AddValue("vesselType", vessel.vesselType.ToString());

			json.AddValue("longitude", vessel.longitude);
			json.AddValue("latitude", vessel.latitude);
			json.AddValue("altitude", vessel.altitude);

			if (buildType == BuildType.BUILD_FULL)
			{
				json.AddValue("isCommandable", vessel.isCommandable);
				json.AddValue("missionTime", vessel.missionTime);
				json.AddValue("landedAt", vessel.landedAt);
				json.AddValue("landed", vessel.Landed);
				json.AddValue("isEVA", vessel.isEVA);

				json.AddValue("crewCapacity", vessel.GetCrewCapacity());
				json.AddValue("crewCount", vessel.GetCrewCount());

				json.StartObject("crew");
				foreach (ProtoCrewMember crewMember in vessel.GetVesselCrew())
				{
					json.StartObject(crewMember.name);
					json.AddValue("name", crewMember.name);
					json.AddValue("courage", crewMember.courage);
					json.AddValue("stupidity", crewMember.stupidity);
					json.AddValue("isBadass", crewMember.isBadass);
					json.EndObject();
				}
				json.EndObject();

				json.AddValue("currentStage", vessel.currentStage);
				json.AddValue("centerOfMass", vessel.CoM);

				json.AddValue("verticalSpeed", vessel.verticalSpeed);
				json.AddValue("surfaceSpeed", vessel.srfSpeed);
				json.AddValue("horizontalSurfaceSpeed", vessel.horizontalSrfSpeed);
				json.AddValue("staticPressure", vessel.staticPressure);
				json.AddValue("atmosphericDensity", vessel.atmDensity);

				json.AddValue("upAxis", vessel.upAxis);

				json.AddValue("acceleration", vessel.acceleration);
				json.AddValue("geeForce", vessel.geeForce);
				json.AddValue("angularVelocity", vessel.angularVelocity);
				json.AddValue("specificAcceleration", vessel.specificAcceleration);

				json.AddValue("orbitVelocity", vessel.obt_velocity);
				json.AddValue("surfaceVelocity", vessel.srf_velocity);

				json.AddValue("heightFromSurface", vessel.GetHeightFromSurface());
				json.AddValue("heightFromTerrain", vessel.GetHeightFromTerrain());
				json.AddValue("terrainNormal", vessel.terrainNormal);
				json.AddValue("terrainAltitude", vessel.terrainAltitude);

				json.AddValue("id", vessel.id.ToString());

				json.StartObject("orbit");
				BuildOrbit(json, vessel.orbit);
				json.EndObject();
			}

			json.StartObject("celestialBody");
			BuildCelestialBody(json, vessel.mainBody, BuildType.BUILD_INDEX);
			json.EndObject();
		}

		private void BuildOrbit(JsonBuilder json, Orbit orbit)
		{
			json.AddValue("apoapsisAltitude", orbit.ApA);
			json.AddValue("apoapsisRadius", orbit.ApR);
			json.AddValue("periapsisAltitude", orbit.PeA);
			json.AddValue("periapsisRadius", orbit.PeR);
			json.AddValue("semiLatusRectum", orbit.semiLatusRectum);
			json.AddValue("semiMinorAxis", orbit.semiMinorAxis);
			json.AddValue("altitude", orbit.altitude);
			json.AddValue("argumentOfPeriapsis", orbit.argumentOfPeriapsis);

			json.AddValue("clAppr", orbit.ClAppr);
			json.AddValue("clEctr1", orbit.ClEctr1);
			json.AddValue("clEctr2", orbit.ClEctr2);
			if (orbit.closestEncounterBody != null)
			{
				json.StartObject("closestEncounterBody");
				BuildCelestialBody(json, orbit.closestEncounterBody, BuildType.BUILD_INDEX);
				json.EndObject();

				// TODO: closestEncounterPath is another orbit.
			}
			json.AddValue("crAppr", orbit.CrAppr);

			json.AddValue("eccentricAnomaly", orbit.eccentricAnomaly);
			json.AddValue("eccentricity", orbit.eccentricity);
			json.AddValue("eccVec", orbit.eccVec);
			json.AddValue("epoch", orbit.epoch);
		}

		private void BuildCelestialBody(JsonBuilder json, CelestialBody body, BuildType buildType = BuildType.BUILD_FULL)
		{
			json.AddValue("name", body.name);
			json.AddValue("flightsGlobalIndex", body.flightGlobalsIndex);

			if (buildType == BuildType.BUILD_FULL)
			{
				// TODO: Its full information.
			}
		}

		private void BuildTransform(JsonBuilder json, Transform transform)
		{
			json.AddValue("position", transform.position);
			json.AddValue("forward", transform.forward);
			json.AddValue("right", transform.right);
			json.AddValue("up", transform.up);
		}
	}
}

