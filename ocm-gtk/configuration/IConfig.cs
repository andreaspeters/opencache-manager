// 
//  Copyright 2011  Kyle Campbell
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;

namespace ocmgtk
{
	public enum SolvedMode {ALL, PUZZLES, NONE};
	public interface IConfig
	{
		SolvedMode SolvedModeState {get;set;}
		double LastLat {get;set;}
		double LastLon {get;set;}
		string LastName {get;set;}
		bool UseDirectEntryMode {get;set;}
		double HomeLat {get;set;}
		double HomeLon {get;set;}
		int MapPoints {get;set;}
		string OwnerID {get;set;}
		string OwnerID2 {get;set;}
		string OwnerID3 {get;set;}
		string OwnerID4 {get;set;}
		bool ImperialUnits {get;set;}
		int WindowWidth {get;set;}
		int WindowHeight {get;set;}
		int VBarPosition {get;set;}
		int HBarPosition {get;set;}
		string MapType {get;set;}
		string DBFile {get;set;}
		string DataDirectory {get;set;}
		string ImportDirectory {get;set;}
		bool UseGPSD {get;set;}
		int GPSDPoll {get;set;}
		bool GPSDAutoMoveMap {get;set;}
		string StartupFilter {get;set;}
		bool ShowNearby {get;set;}
		bool ShowAllChildren {get;set;}
		string GPSProf {get;set;}
		bool IgnoreWaypointPrefixes {get;set;}
		bool CheckForUpdates {get;set;}
		DateTime NextUpdateCheck {get;set;}
		int UpdateInterval {get;set;}
		bool AutoCloseWindows{get;set;}
		bool AutoSelectCacheFromMap{get;set;}
		bool ShowDNFIcon{get;set;}
		bool UseOfflineLogging{get;set;}
		string FieldNotesFile{get;}
		bool ImportIgnoreExtraFields{get;set;}
		bool ImportPreventStatusOverwrite{get;set;}
		bool ImportPurgeOldLogs{get;set;}
		bool ImportDeleteFiles{get;set;}
		int ExportLimitCaches{get;set;}
		bool ExportChildren{get;set;}
		bool ExportPaperlessOptions{get;set;}
		bool ExportExtraFields{get;set;}
		bool ExportCustomSymbols{get;set;}
		int ExportLimitLogs{get;set;}
		bool ExportIncludeAttributes{get;set;}
		bool ExportAsPlainText{get;set;}
		bool WizardDone{get;set;}
		string ExportPOIFile{get;set;}
		ocmengine.WaypointNameMode ExportPOINameMode{get;set;}
		ocmengine.WaypointDescMode ExportPOIDescMode{get;set;}
		string ExportPOICategory{get;set;}
		int ExportPOICacheLimit{get;set;}
		bool ExportPOIIncludeChildren{get;set;}
		string ExportPOIBitmap{get;set;}
		int ExportPOILogLimit{get;set;}
		bool ExportPOIForcePlain{get;set;}
		double ExportPOIProxDist{get;set;}
		string ExportPOIProxUnits{get;set;}
		ocmengine.WaypointDescMode ExportWaypointDescMode{get;set;}
		ocmengine.WaypointNameMode ExportWaypointNameMode{get;set;}
		List<MapDescription> OpenLayerMaps{get;set;}
		bool ShowDiffTerrIcon{get;set;}
		bool EnableTraceLog{get;set;}
		bool ClearTraceLog{get;set;}
		bool MapPopups{get;set;}
		bool ShowStaleCaches{get;set;}
		int StaleCacheInterval{get;set;}
		bool ShowNewCaches{get;set;}
		int NewCacheInterval{get;set;}
		bool ShowRecentDNF{get;set;}
		
		DateTime LastGPSFieldNoteScan{get;set;}
		string ImportBookmarkList{get;set;}		
		void CheckForDefaultGPS(GPSProfileList list, OCMMainWindow win);
	}
}
