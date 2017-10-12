// 
//  Copyright 2010  Kyle Campbell
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


	[System.ComponentModel.ToolboxItem(true)]
	public partial class OCMQueryPage5 : Gtk.Bin
	{

		public OCMQueryPage5 ()
		{
			this.Build ();
		}
		
		public List<string> IncludeAttributes
		{
			get
			{
				List<string> attrs = new List<string>();
				if (winterAttr.IsFiltered && winterAttr.IsIncluded)
					attrs.Add(winterAttr.AttributeName);
				if (maintenanceAttr.IsFiltered && maintenanceAttr.IsIncluded)
					attrs.Add(maintenanceAttr.AttributeName);
				if (nightAttr.IsFiltered && nightAttr.IsIncluded)
					attrs.Add(nightAttr.AttributeName);
				if (beaconFilt.IsFiltered && beaconFilt.IsIncluded)
					attrs.Add(beaconFilt.AttributeName);
				if (dogFilt.IsFiltered && dogFilt.IsIncluded)
					attrs.Add(dogFilt.AttributeName);
				if (bikeFilt.IsFiltered && bikeFilt.IsIncluded)
					attrs.Add(bikeFilt.AttributeName);
				if (feeFilt.IsFiltered && feeFilt.IsIncluded)
					attrs.Add(feeFilt.AttributeName);
				if (kidFilt.IsFiltered && kidFilt.IsIncluded)
					attrs.Add(kidFilt.AttributeName);
				if (fireFilt.IsFiltered && fireFilt.IsIncluded)
					attrs.Add(fireFilt.AttributeName);
				if (timeFilt.IsFiltered && timeFilt.IsIncluded)
					attrs.Add(timeFilt.AttributeName);
				if (swimFilt.IsFiltered && swimFilt.IsIncluded)
					attrs.Add(swimFilt.AttributeName);
				if (parkFilt.IsFiltered && parkFilt.IsIncluded)
					attrs.Add(parkFilt.AttributeName);
				if (strollFilt.IsFiltered && strollFilt.IsIncluded)
					attrs.Add(strollFilt.AttributeName);
				if (wchairFilt.IsFiltered && wchairFilt.IsIncluded)
					attrs.Add(wchairFilt.AttributeName);
				if (fpuzzFilt.IsFiltered && fpuzzFilt.IsIncluded)
					attrs.Add(fpuzzFilt.AttributeName);
				if (shikeFilt.IsFiltered && shikeFilt.IsIncluded)
					attrs.Add(shikeFilt.AttributeName);
				if (scubaFilt.IsFiltered && scubaFilt.IsIncluded)
					attrs.Add(scubaFilt.AttributeName);
				if (mineFilt.IsFiltered && mineFilt.IsIncluded)
					attrs.Add(mineFilt.AttributeName);
				if (cliffFilt.IsFiltered && cliffFilt.IsIncluded)
					attrs.Add(cliffFilt.AttributeName);
				if (climbGearFilt.IsFiltered && climbGearFilt.IsIncluded)
					attrs.Add(climbGearFilt.AttributeName);
				if (skiFilt.IsFiltered && skiFilt.IsIncluded)
					attrs.Add(skiFilt.AttributeName);
				if (dangerFilt.IsFiltered && dangerFilt.IsIncluded)
					attrs.Add(dangerFilt.AttributeName);
				if (busFilt.IsFiltered && busFilt.IsIncluded)
					attrs.Add(busFilt.AttributeName);
				if (cowFilt.IsFiltered && cowFilt.IsIncluded)
					attrs.Add(cowFilt.AttributeName);
				if (allTimeFilt.IsFiltered && allTimeFilt.IsIncluded)
					attrs.Add(allTimeFilt.AttributeName);
				if (spyFilt.IsFiltered && spyFilt.IsIncluded)
					attrs.Add(spyFilt.AttributeName);
				if (sshoeFilt.IsFiltered && sshoeFilt.IsIncluded)
					attrs.Add(sshoeFilt.AttributeName);
				if (toolFilt.IsFiltered && toolFilt.IsIncluded)
					attrs.Add(toolFilt.AttributeName);
				if (thornFilt.IsFiltered && thornFilt.IsIncluded)
					attrs.Add(thornFilt.AttributeName);
				if (uvFilt.IsFiltered && uvFilt.IsIncluded)
					attrs.Add(uvFilt.AttributeName);
				if (boatFilt.IsFiltered && boatFilt.IsIncluded)
					attrs.Add(boatFilt.AttributeName);
				if (abandFilt.IsFiltered && abandFilt.IsIncluded)
					attrs.Add(abandFilt.AttributeName);
				if (campFilt.IsFiltered && campFilt.IsIncluded)
					attrs.Add(campFilt.AttributeName);
				if (animalFilt.IsFiltered && animalFilt.IsIncluded)
					attrs.Add(animalFilt.AttributeName);
				if (dclimbFilt.IsFiltered && dclimbFilt.IsIncluded)
					attrs.Add(dclimbFilt.AttributeName);
				if (drinkFilt.IsFiltered && drinkFilt.IsIncluded)
					attrs.Add(drinkFilt.AttributeName);
				if (lightFilt.IsFiltered && lightFilt.IsIncluded)
					attrs.Add(lightFilt.AttributeName);
				if (foodFild.IsFiltered && foodFild.IsIncluded)
					attrs.Add(foodFild.AttributeName);
				if (fuelFilt.IsFiltered && fuelFilt.IsIncluded)
					attrs.Add(fuelFilt.AttributeName);
				if (horseFilt.IsFiltered && horseFilt.IsIncluded)
					attrs.Add(horseFilt.AttributeName);
				if (huntFilt.IsFiltered && huntFilt.IsIncluded)
					attrs.Add(huntFilt.AttributeName);
				if (wadeFilt.IsFiltered && wadeFilt.IsIncluded)
					attrs.Add(wadeFilt.AttributeName);
				if (motoFilt.IsFiltered && motoFilt.IsIncluded)
					attrs.Add(motoFilt.AttributeName);
				if (nightCFilt.IsFiltered && nightCFilt.IsIncluded)
					attrs.Add(nightCFilt.AttributeName);
				if (pgrabFilt.IsFiltered && pgrabFilt.IsIncluded)
					attrs.Add(pgrabFilt.AttributeName);
				if (viewFilt.IsFiltered && viewFilt.IsIncluded)
					attrs.Add(viewFilt.AttributeName);
				if (shortFilt.IsFiltered && shortFilt.IsIncluded)
					attrs.Add(shortFilt.AttributeName);
				if (mediumFilt.IsFiltered && mediumFilt.IsIncluded)
					attrs.Add(mediumFilt.AttributeName);
				if (longFilt.IsFiltered && longFilt.IsIncluded)
					attrs.Add(longFilt.AttributeName);
				if (plantFilt.IsFiltered && plantFilt.IsIncluded)
					attrs.Add(plantFilt.AttributeName);
				if (tickFilt.IsFiltered && tickFilt.IsIncluded)
					attrs.Add(tickFilt.AttributeName);
				if (picnicFilt.IsFiltered && picnicFilt.IsIncluded)
					attrs.Add(picnicFilt.AttributeName);
				if (washFilt.IsFiltered && washFilt.IsIncluded)
					attrs.Add(washFilt.AttributeName);
				if (phoneFilt.IsFiltered && phoneFilt.IsIncluded)
					attrs.Add(phoneFilt.AttributeName);
				if (rvFilt.IsFiltered && rvFilt.IsIncluded)
					attrs.Add(rvFilt.AttributeName);
				if (quadFilt.IsFiltered && quadFilt.IsIncluded)
					attrs.Add(quadFilt.AttributeName);
				if (offrFilt.IsFiltered && offrFilt.IsIncluded)
					attrs.Add(offrFilt.AttributeName);
				if (skidooFilt.IsFiltered && skidooFilt.IsIncluded)
					attrs.Add(skidooFilt.AttributeName);
				return attrs;
			}
			set
			{
				if (value.Contains(winterAttr.AttributeName))
					winterAttr.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(maintenanceAttr.AttributeName))
					maintenanceAttr.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(nightAttr.AttributeName))
					nightAttr.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(beaconFilt.AttributeName))
					beaconFilt.SetState(AttributeFilterWidget.AttrState.YES);	
				if (value.Contains(dogFilt.AttributeName))
					dogFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(bikeFilt.AttributeName))
					bikeFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(feeFilt.AttributeName))
					feeFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(kidFilt.AttributeName))
					kidFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(fireFilt.AttributeName))
					fireFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(timeFilt.AttributeName))
					timeFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(swimFilt.AttributeName))
					swimFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(parkFilt.AttributeName))
					parkFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(strollFilt.AttributeName))
					strollFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(wchairFilt.AttributeName))
					wchairFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(fpuzzFilt.AttributeName))
					fpuzzFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(shikeFilt.AttributeName))
					shikeFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(scubaFilt.AttributeName))
					scubaFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(mineFilt.AttributeName))
					mineFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(cliffFilt.AttributeName))
					cliffFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(climbGearFilt.AttributeName))
					climbGearFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(skiFilt.AttributeName))
					skiFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(dangerFilt.AttributeName))
					dangerFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(busFilt.AttributeName))
					busFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(cowFilt.AttributeName))
					cowFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(allTimeFilt.AttributeName))
					allTimeFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(spyFilt.AttributeName))
					spyFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(sshoeFilt.AttributeName))
					sshoeFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(toolFilt.AttributeName))
					toolFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(thornFilt.AttributeName))
					thornFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(uvFilt.AttributeName))
					uvFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(boatFilt.AttributeName))
					boatFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(abandFilt.AttributeName))
					abandFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(campFilt.AttributeName))
					campFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(animalFilt.AttributeName))
					animalFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(dclimbFilt.AttributeName))
					dclimbFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(drinkFilt.AttributeName))
					drinkFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(lightFilt.AttributeName))
					lightFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(foodFild.AttributeName))
					foodFild.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(fuelFilt.AttributeName))
					fuelFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(horseFilt.AttributeName))
					horseFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(huntFilt.AttributeName))
					huntFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(wadeFilt.AttributeName))
					wadeFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(motoFilt.AttributeName))
					motoFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(nightCFilt.AttributeName))
					nightCFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(pgrabFilt.AttributeName))
					pgrabFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(viewFilt.AttributeName))
					viewFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(mediumFilt.AttributeName))
					mediumFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(longFilt.AttributeName))
					longFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(plantFilt.AttributeName))
					plantFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(tickFilt.AttributeName))
					tickFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(picnicFilt.AttributeName))
					picnicFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(washFilt.AttributeName))
					washFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(phoneFilt.AttributeName))
					phoneFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(rvFilt.AttributeName))
					rvFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(quadFilt.AttributeName))
					quadFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(offrFilt.AttributeName))
					offrFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(skidooFilt.AttributeName))
					skidooFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(shortFilt.AttributeName))
					shortFilt.SetState(AttributeFilterWidget.AttrState.YES);
			}
		}
		
		public List<string> MustNotHaveIncludeAttributes
		{
			get
			{
				List<string> attrs = new List<string>();
				if (winterEAttr.IsFiltered && winterEAttr.IsIncluded)
					attrs.Add(winterEAttr.AttributeName);
				if (maintenanceEAttr.IsFiltered && maintenanceEAttr.IsIncluded)
					attrs.Add(maintenanceEAttr.AttributeName);
				if (nightEAttr.IsFiltered && nightEAttr.IsIncluded)
					attrs.Add(nightEAttr.AttributeName);
				if (beaconEFilt.IsFiltered && beaconEFilt.IsIncluded)
					attrs.Add(beaconEFilt.AttributeName);
				if (dogEFilt.IsFiltered && dogEFilt.IsIncluded)
					attrs.Add(dogEFilt.AttributeName);
				if (bikeEFilt.IsFiltered && bikeEFilt.IsIncluded)
					attrs.Add(bikeEFilt.AttributeName);
				if (feeEFilt.IsFiltered && feeEFilt.IsIncluded)
					attrs.Add(feeEFilt.AttributeName);
				if (kidEFilt.IsFiltered && kidEFilt.IsIncluded)
					attrs.Add(kidEFilt.AttributeName);
				if (fireEFilt.IsFiltered && fireEFilt.IsIncluded)
					attrs.Add(fireEFilt.AttributeName);
				if (timeEFilt.IsFiltered && timeEFilt.IsIncluded)
					attrs.Add(timeEFilt.AttributeName);
				if (swimEFilt.IsFiltered && swimEFilt.IsIncluded)
					attrs.Add(swimEFilt.AttributeName);
				if (parkEFilt.IsFiltered && parkEFilt.IsIncluded)
					attrs.Add(parkEFilt.AttributeName);
				if (strollEFilt.IsFiltered && strollEFilt.IsIncluded)
					attrs.Add(strollEFilt.AttributeName);
				if (wchairEFilt.IsFiltered && wchairEFilt.IsIncluded)
					attrs.Add(wchairEFilt.AttributeName);
				if (fpuzzEFilt.IsFiltered && fpuzzEFilt.IsIncluded)
					attrs.Add(fpuzzEFilt.AttributeName);
				if (shikeEFilt1.IsFiltered && shikeEFilt1.IsIncluded)
					attrs.Add(shikeEFilt1.AttributeName);
				if (scubaEFilt.IsFiltered && scubaEFilt.IsIncluded)
					attrs.Add(scubaEFilt.AttributeName);
				if (mineEFilt.IsFiltered && mineEFilt.IsIncluded)
					attrs.Add(mineEFilt.AttributeName);
				if (cliffEFilt.IsFiltered && cliffEFilt.IsIncluded)
					attrs.Add(cliffEFilt.AttributeName);
				if (climbGearEFilt.IsFiltered && climbGearEFilt.IsIncluded)
					attrs.Add(climbGearEFilt.AttributeName);
				if (skiEFilt.IsFiltered && skiEFilt.IsIncluded)
					attrs.Add(skiEFilt.AttributeName);
				if (dangerEFilt.IsFiltered && dangerEFilt.IsIncluded)
					attrs.Add(dangerEFilt.AttributeName);
				if (busEFilt.IsFiltered && busEFilt.IsIncluded)
					attrs.Add(busEFilt.AttributeName);
				if (cowEFilt.IsFiltered && cowEFilt.IsIncluded)
					attrs.Add(cowEFilt.AttributeName);
				if (allTimeEFilt.IsFiltered && allTimeEFilt.IsIncluded)
					attrs.Add(allTimeEFilt.AttributeName);
				if (spyEFilt.IsFiltered && spyEFilt.IsIncluded)
					attrs.Add(spyEFilt.AttributeName);
				if (sshoeEFilt.IsFiltered && sshoeEFilt.IsIncluded)
					attrs.Add(sshoeEFilt.AttributeName);
				if (toolEFilt.IsFiltered && toolEFilt.IsIncluded)
					attrs.Add(toolEFilt.AttributeName);
				if (thornEFilt.IsFiltered && thornEFilt.IsIncluded)
					attrs.Add(thornEFilt.AttributeName);
				if (uvEFilt.IsFiltered && uvEFilt.IsIncluded)
					attrs.Add(uvEFilt.AttributeName);
				if (boatEFilt.IsFiltered && boatEFilt.IsIncluded)
					attrs.Add(boatEFilt.AttributeName);
				if (abandEFilt.IsFiltered && abandEFilt.IsIncluded)
					attrs.Add(abandEFilt.AttributeName);
				if (campEFilt.IsFiltered && campEFilt.IsIncluded)
					attrs.Add(campEFilt.AttributeName);
				if (animalEFilt.IsFiltered && animalEFilt.IsIncluded)
					attrs.Add(animalEFilt.AttributeName);
				if (dclimbEFilt.IsFiltered && dclimbEFilt.IsIncluded)
					attrs.Add(dclimbEFilt.AttributeName);
				if (drinkEFilt.IsFiltered && drinkEFilt.IsIncluded)
					attrs.Add(drinkEFilt.AttributeName);
				if (lightEFilt.IsFiltered && lightEFilt.IsIncluded)
					attrs.Add(lightEFilt.AttributeName);
				if (foodEFilt.IsFiltered && foodEFilt.IsIncluded)
					attrs.Add(foodEFilt.AttributeName);
				if (fuelEFilt.IsFiltered && fuelEFilt.IsIncluded)
					attrs.Add(fuelEFilt.AttributeName);
				if (horseEFilt.IsFiltered && horseEFilt.IsIncluded)
					attrs.Add(horseEFilt.AttributeName);
				if (huntEFilt.IsFiltered && huntEFilt.IsIncluded)
					attrs.Add(huntEFilt.AttributeName);
				if (wadeEFilt.IsFiltered && wadeEFilt.IsIncluded)
					attrs.Add(wadeEFilt.AttributeName);
				if (motoEFilt.IsFiltered && motoEFilt.IsIncluded)
					attrs.Add(motoEFilt.AttributeName);
				if (nightCEFilt.IsFiltered && nightCEFilt.IsIncluded)
					attrs.Add(nightCEFilt.AttributeName);
				if (pgrabEFilt1.IsFiltered && pgrabEFilt1.IsIncluded)
					attrs.Add(pgrabEFilt1.AttributeName);
				if (viewEFilt.IsFiltered && viewEFilt.IsIncluded)
					attrs.Add(viewEFilt.AttributeName);
				if (shortEFilt.IsFiltered && shortEFilt.IsIncluded)
					attrs.Add(shortEFilt.AttributeName);
				if (mediumEFilt.IsFiltered && mediumEFilt.IsIncluded)
					attrs.Add(mediumEFilt.AttributeName);
				if (longEFilt.IsFiltered && longEFilt.IsIncluded)
					attrs.Add(longEFilt.AttributeName);
				if (plantEFilt.IsFiltered && plantEFilt.IsIncluded)
					attrs.Add(plantEFilt.AttributeName);
				if (tickEFilt.IsFiltered && tickEFilt.IsIncluded)
					attrs.Add(tickEFilt.AttributeName);
				if (picnicEFilt.IsFiltered && picnicEFilt.IsIncluded)
					attrs.Add(picnicEFilt.AttributeName);
				if (washEFilt.IsFiltered && washEFilt.IsIncluded)
					attrs.Add(washEFilt.AttributeName);
				if (phoneEFilt.IsFiltered && phoneEFilt.IsIncluded)
					attrs.Add(phoneEFilt.AttributeName);
				if (rvEFilt.IsFiltered && rvEFilt.IsIncluded)
					attrs.Add(rvEFilt.AttributeName);
				if (quadEFilt.IsFiltered && quadEFilt.IsIncluded)
					attrs.Add(quadEFilt.AttributeName);
				if (offrEFilt.IsFiltered && offrEFilt.IsIncluded)
					attrs.Add(offrEFilt.AttributeName);
				if (skidooFilt1.IsFiltered && skidooFilt1.IsIncluded)
					attrs.Add(skidooFilt1.AttributeName);
				return attrs;
			}
			set
			{
				if (value.Contains(winterEAttr.AttributeName))
					winterEAttr.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(maintenanceEAttr.AttributeName))
					maintenanceEAttr.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(nightEAttr.AttributeName))
					nightEAttr.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(beaconEFilt.AttributeName))
					beaconEFilt.SetState(AttributeFilterWidget.AttrState.YES);	
				if (value.Contains(dogEFilt.AttributeName))
					dogEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(bikeEFilt.AttributeName))
					bikeEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(feeEFilt.AttributeName))
					feeEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(kidEFilt.AttributeName))
					kidEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(fireEFilt.AttributeName))
					fireEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(swimEFilt.AttributeName))
					swimEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(parkEFilt.AttributeName))
					parkEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(strollEFilt.AttributeName))
					strollEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(wchairEFilt.AttributeName))
					wchairEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(fpuzzEFilt.AttributeName))
					fpuzzEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(shikeEFilt1.AttributeName))
					shikeEFilt1.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(scubaEFilt.AttributeName))
					scubaEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(mineEFilt.AttributeName))
					mineEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(cliffEFilt.AttributeName))
					cliffEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(climbGearEFilt.AttributeName))
					climbGearEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(skiEFilt.AttributeName))
					skiEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(dangerEFilt.AttributeName))
					dangerEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(busEFilt.AttributeName))
					busEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(cowEFilt.AttributeName))
					cowEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(allTimeEFilt.AttributeName))
					allTimeEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(spyEFilt.AttributeName))
					spyEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(sshoeEFilt.AttributeName))
					sshoeEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(toolEFilt.AttributeName))
					toolEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(thornEFilt.AttributeName))
					thornEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(uvEFilt.AttributeName))
					uvEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(boatEFilt.AttributeName))
					boatEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(abandEFilt.AttributeName))
					abandEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(campEFilt.AttributeName))
					campEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(animalEFilt.AttributeName))
					animalEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(dclimbEFilt.AttributeName))
					dclimbEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(drinkEFilt.AttributeName))
					drinkEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(foodEFilt.AttributeName))
					foodEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(fuelEFilt.AttributeName))
					fuelEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(horseEFilt.AttributeName))
					horseEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(huntEFilt.AttributeName))
					huntEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(wadeEFilt.AttributeName))
					wadeEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(motoEFilt.AttributeName))
					motoEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(nightCEFilt.AttributeName))
					nightCEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(pgrabEFilt1.AttributeName))
					pgrabEFilt1.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(viewEFilt.AttributeName))
					viewEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(shortEFilt.AttributeName))
					shortEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(mediumEFilt.AttributeName))
					mediumEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(longEFilt.AttributeName))
					longEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(plantEFilt.AttributeName))
					plantEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(tickEFilt.AttributeName))
					tickEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(picnicEFilt.AttributeName))
					picnicEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(washEFilt.AttributeName))
					washEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(phoneEFilt.AttributeName))
					phoneEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(rvEFilt.AttributeName))
					rvEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(quadEFilt.AttributeName))
					quadEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(offrEFilt.AttributeName))
					offrEFilt.SetState(AttributeFilterWidget.AttrState.YES);
				if (value.Contains(skidooFilt1.AttributeName))
					skidooFilt1.SetState(AttributeFilterWidget.AttrState.YES);
			}
		}
		
		public List<string> MustHaveNegAttributes
		{
			get
			{
				List<string> attrs = new List<string>();
				if (winterAttr.IsFiltered && !winterAttr.IsIncluded)
					attrs.Add(winterAttr.AttributeName);
				if (nightAttr.IsFiltered && !nightAttr.IsIncluded)
					attrs.Add(nightAttr.AttributeName);
				if (dogFilt.IsFiltered && !dogFilt.IsIncluded)
					attrs.Add(dogFilt.AttributeName);
				if (bikeFilt.IsFiltered && !bikeFilt.IsIncluded)
					attrs.Add(bikeFilt.AttributeName);
				if (kidFilt.IsFiltered && !kidFilt.IsIncluded)
					attrs.Add(kidFilt.AttributeName);
				if (fireFilt.IsFiltered && !fireFilt.IsIncluded)
					attrs.Add(fireFilt.AttributeName);
				if (timeFilt.IsFiltered && !timeFilt.IsIncluded)
					attrs.Add(timeFilt.AttributeName);
				if (parkFilt.IsFiltered && !parkFilt.IsIncluded)
					attrs.Add(parkFilt.AttributeName);
				if (strollFilt.IsFiltered && !strollFilt.IsIncluded)
					attrs.Add(strollFilt.AttributeName);
				if (wchairFilt.IsFiltered && !wchairFilt.IsIncluded)
					attrs.Add(wchairFilt.AttributeName);
				if (fpuzzFilt.IsFiltered && !fpuzzFilt.IsIncluded)
					attrs.Add(fpuzzFilt.AttributeName);
				if (shikeFilt.IsFiltered && !shikeFilt.IsIncluded)
					attrs.Add(shikeFilt.AttributeName);
				if (allTimeFilt.IsFiltered && !allTimeFilt.IsIncluded)
					attrs.Add(allTimeFilt.AttributeName);
				if (spyFilt.IsFiltered && !spyFilt.IsIncluded)
					attrs.Add(spyFilt.AttributeName);
				if (abandFilt.IsFiltered && !abandFilt.IsIncluded)
					attrs.Add(abandFilt.AttributeName);
				if (campFilt.IsFiltered && !campFilt.IsIncluded)
					attrs.Add(campFilt.AttributeName);
				if (animalFilt.IsFiltered && !animalFilt.IsIncluded)
					attrs.Add(animalFilt.AttributeName);
				if (dclimbFilt.IsFiltered && !dclimbFilt.IsIncluded)
					attrs.Add(dclimbFilt.AttributeName);
				if (drinkFilt.IsFiltered && !drinkFilt.IsIncluded)
					attrs.Add(drinkFilt.AttributeName);
				if (foodFild.IsFiltered && !foodFild.IsIncluded)
					attrs.Add(foodFild.AttributeName);
				if (fuelFilt.IsFiltered && !fuelFilt.IsIncluded)
					attrs.Add(fuelFilt.AttributeName);
				if (horseFilt.IsFiltered && !horseFilt.IsIncluded)
					attrs.Add(horseFilt.AttributeName);
				if (motoFilt.IsFiltered && !motoFilt.IsIncluded)
					attrs.Add(motoFilt.AttributeName);
				if (viewFilt.IsFiltered && !viewFilt.IsIncluded)
					attrs.Add(viewFilt.AttributeName);
				if (shortFilt.IsFiltered && !shortFilt.IsIncluded)
					attrs.Add(shortFilt.AttributeName);
				if (mediumFilt.IsFiltered && !mediumFilt.IsIncluded)
					attrs.Add(mediumFilt.AttributeName);
				if (longFilt.IsFiltered && !longFilt.IsIncluded)
					attrs.Add(longFilt.AttributeName);
				if (plantFilt.IsFiltered && !plantFilt.IsIncluded)
					attrs.Add(plantFilt.AttributeName);
				if (picnicFilt.IsFiltered && !picnicFilt.IsIncluded)
					attrs.Add(picnicFilt.AttributeName);
				if (washFilt.IsFiltered && !washFilt.IsIncluded)
					attrs.Add(washFilt.AttributeName);
				if (phoneFilt.IsFiltered && !phoneFilt.IsIncluded)
					attrs.Add(phoneFilt.AttributeName);
				if (rvFilt.IsFiltered && !rvFilt.IsIncluded)
					attrs.Add(rvFilt.AttributeName);
				if (quadFilt.IsFiltered && !quadFilt.IsIncluded)
					attrs.Add(quadFilt.AttributeName);
				if (offrFilt.IsFiltered && !offrFilt.IsIncluded)
					attrs.Add(offrFilt.AttributeName);
				if (skidooFilt.IsFiltered && !skidooFilt.IsIncluded)
					attrs.Add(skidooFilt.AttributeName);
				if (pgrabFilt.IsFiltered && !pgrabFilt.IsIncluded)
					attrs.Add(pgrabFilt.AttributeName);
				if (nightCFilt.IsFiltered && !nightCFilt.IsIncluded)
					attrs.Add(nightCFilt.AttributeName);
				return attrs;
			}
			set
			{	
				if (value.Contains(winterAttr.AttributeName))
					winterAttr.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(maintenanceAttr.AttributeName))
					maintenanceAttr.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(nightAttr.AttributeName))
					nightAttr.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(beaconFilt.AttributeName))
					beaconFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(dogFilt.AttributeName))
					dogFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(bikeFilt.AttributeName))
					bikeFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(timeFilt.AttributeName))
					timeFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(kidFilt.AttributeName))
					kidFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(parkFilt.AttributeName))
					parkFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(strollFilt.AttributeName))
					strollFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(wchairFilt.AttributeName))
					wchairFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(fpuzzFilt.AttributeName))
					fpuzzFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(shikeFilt.AttributeName))
					shikeFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(allTimeFilt.AttributeName))
					allTimeFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(spyFilt.AttributeName))
					spyFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(abandFilt.AttributeName))
					abandFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(campFilt.AttributeName))
					campFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(animalFilt.AttributeName))
					animalFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(dclimbFilt.AttributeName))
					dclimbFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(drinkFilt.AttributeName))
					drinkFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(foodFild.AttributeName))
					foodFild.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(fuelFilt.AttributeName))
					fuelFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(horseFilt.AttributeName))
					horseFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(motoFilt.AttributeName))
					motoFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(viewFilt.AttributeName))
					viewFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(shortFilt.AttributeName))
					shortFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(mediumFilt.AttributeName))
					mediumFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(longFilt.AttributeName))
					longFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(plantFilt.AttributeName))
					plantFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(picnicFilt.AttributeName))
					picnicFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(washFilt.AttributeName))
					washFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(phoneFilt.AttributeName))
					phoneFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(rvFilt.AttributeName))
					rvFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(quadFilt.AttributeName))
					quadFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(offrFilt.AttributeName))
					offrFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(skidooFilt.AttributeName))
					skidooFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(pgrabFilt.AttributeName))
					pgrabFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(nightCFilt.AttributeName))
					nightCFilt.SetState(AttributeFilterWidget.AttrState.NO);
			}
		}
		
		public List<string> MustNotHaveNegAttributes
		{
			get
			{
				List<string> attrs = new List<string>();
				if (winterEAttr.IsFiltered && !winterEAttr.IsIncluded)
					attrs.Add(winterEAttr.AttributeName);
				if (nightEAttr.IsFiltered && !nightAttr.IsIncluded)
					attrs.Add(nightEAttr.AttributeName);
				if (dogEFilt.IsFiltered && !dogEFilt.IsIncluded)
					attrs.Add(dogEFilt.AttributeName);
				if (bikeEFilt.IsFiltered && !bikeEFilt.IsIncluded)
					attrs.Add(bikeEFilt.AttributeName);
				if (kidEFilt.IsFiltered && !kidEFilt.IsIncluded)
					attrs.Add(kidEFilt.AttributeName);
				if (fireEFilt.IsFiltered && !fireEFilt.IsIncluded)
					attrs.Add(fireEFilt.AttributeName);
				if (timeEFilt.IsFiltered && !timeEFilt.IsIncluded)
					attrs.Add(timeEFilt.AttributeName);
				if (parkEFilt.IsFiltered && !parkEFilt.IsIncluded)
					attrs.Add(parkEFilt.AttributeName);
				if (strollEFilt.IsFiltered && !strollEFilt.IsIncluded)
					attrs.Add(strollEFilt.AttributeName);
				if (wchairEFilt.IsFiltered && !wchairEFilt.IsIncluded)
					attrs.Add(wchairEFilt.AttributeName);
				if (fpuzzEFilt.IsFiltered && !fpuzzEFilt.IsIncluded)
					attrs.Add(fpuzzEFilt.AttributeName);
				if (shikeEFilt1.IsFiltered && !shikeEFilt1.IsIncluded)
					attrs.Add(shikeEFilt1.AttributeName);
				if (allTimeEFilt.IsFiltered && !allTimeEFilt.IsIncluded)
					attrs.Add(allTimeEFilt.AttributeName);
				if (spyEFilt.IsFiltered && !spyEFilt.IsIncluded)
					attrs.Add(spyEFilt.AttributeName);
				if (abandEFilt.IsFiltered && !abandEFilt.IsIncluded)
					attrs.Add(abandEFilt.AttributeName);
				if (campEFilt.IsFiltered && !campEFilt.IsIncluded)
					attrs.Add(campEFilt.AttributeName);
				if (animalEFilt.IsFiltered && !animalEFilt.IsIncluded)
					attrs.Add(animalEFilt.AttributeName);
				if (dclimbEFilt.IsFiltered && !dclimbEFilt.IsIncluded)
					attrs.Add(dclimbEFilt.AttributeName);
				if (drinkEFilt.IsFiltered && !drinkEFilt.IsIncluded)
					attrs.Add(drinkEFilt.AttributeName);
				if (fuelEFilt.IsFiltered && !fuelEFilt.IsIncluded)
					attrs.Add(fuelEFilt.AttributeName);
				if (foodEFilt.IsFiltered && !foodEFilt.IsIncluded)
					attrs.Add(foodEFilt.AttributeName);
				if (horseEFilt.IsFiltered && !horseEFilt.IsIncluded)
					attrs.Add(horseEFilt.AttributeName);
				if (motoEFilt.IsFiltered && !motoEFilt.IsIncluded)
					attrs.Add(motoEFilt.AttributeName);
				if (viewEFilt.IsFiltered && !viewEFilt.IsIncluded)
					attrs.Add(viewEFilt.AttributeName);
				if (shortEFilt.IsFiltered && !shortEFilt.IsIncluded)
					attrs.Add(shortEFilt.AttributeName);
				if (mediumEFilt.IsFiltered && !mediumEFilt.IsIncluded)
					attrs.Add(mediumEFilt.AttributeName);
				if (longEFilt.IsFiltered && !longEFilt.IsIncluded)
					attrs.Add(longEFilt.AttributeName);
				if (plantEFilt.IsFiltered && !plantEFilt.IsIncluded)
					attrs.Add(plantEFilt.AttributeName);
				if (picnicEFilt.IsFiltered && !picnicEFilt.IsIncluded)
					attrs.Add(picnicEFilt.AttributeName);
				if (washEFilt.IsFiltered && !washEFilt.IsIncluded)
					attrs.Add(washEFilt.AttributeName);
				if (phoneEFilt.IsFiltered && !phoneEFilt.IsIncluded)
					attrs.Add(phoneEFilt.AttributeName);
				if (rvEFilt.IsFiltered && !rvEFilt.IsIncluded)
					attrs.Add(rvEFilt.AttributeName);
				if (quadEFilt.IsFiltered && !quadEFilt.IsIncluded)
					attrs.Add(quadEFilt.AttributeName);
				if (offrEFilt.IsFiltered && !offrEFilt.IsIncluded)
					attrs.Add(offrEFilt.AttributeName);
				if (skidooFilt1.IsFiltered && !skidooFilt1.IsIncluded)
					attrs.Add(skidooFilt1.AttributeName);
				if (pgrabEFilt1.IsFiltered && !pgrabEFilt1.IsIncluded)
					attrs.Add(pgrabEFilt1.AttributeName);
				if (nightCEFilt.IsFiltered && !nightCEFilt.IsIncluded)
					attrs.Add(nightCEFilt.AttributeName);
				return attrs;
			}
			set
			{	
				if (value.Contains(winterEAttr.AttributeName))
					winterEAttr.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(maintenanceEAttr.AttributeName))
					maintenanceEAttr.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(nightEAttr.AttributeName))
					nightEAttr.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(beaconEFilt.AttributeName))
					beaconEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(dogEFilt.AttributeName))
					dogEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(bikeEFilt.AttributeName))
					bikeEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(kidEFilt.AttributeName))
					kidEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(fireEFilt.AttributeName))
					fireEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(timeEFilt.AttributeName))
					timeEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(parkEFilt.AttributeName))
					parkEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(strollEFilt.AttributeName))
					strollEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(wchairEFilt.AttributeName))
					wchairEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(fpuzzEFilt.AttributeName))
					fpuzzEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(shikeEFilt1.AttributeName))
					shikeEFilt1.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(allTimeEFilt.AttributeName))
					allTimeEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(spyEFilt.AttributeName))
					spyEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(abandEFilt.AttributeName))
					abandEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(campEFilt.AttributeName))
					campEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(animalEFilt.AttributeName))
					animalEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(dclimbEFilt.AttributeName))
					dclimbEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(drinkEFilt.AttributeName))
					drinkEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(foodEFilt.AttributeName))
					foodEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(fuelEFilt.AttributeName))
					fuelEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(horseEFilt.AttributeName))
					horseEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(motoEFilt.AttributeName))
					motoEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(viewEFilt.AttributeName))
					viewEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(shortEFilt.AttributeName))
					shortEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(mediumEFilt.AttributeName))
					mediumEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(longEFilt.AttributeName))
					longEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(plantEFilt.AttributeName))
					plantEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(picnicEFilt.AttributeName))
					picnicEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(washEFilt.AttributeName))
					washEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(phoneEFilt.AttributeName))
					phoneEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(rvEFilt.AttributeName))
					rvEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(quadEFilt.AttributeName))
					quadEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(offrEFilt.AttributeName))
					offrEFilt.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(skidooFilt1.AttributeName))
					skidooFilt1.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(pgrabEFilt1.AttributeName))
					pgrabEFilt1.SetState(AttributeFilterWidget.AttrState.NO);
				if (value.Contains(nightCEFilt.AttributeName))
					nightCEFilt.SetState(AttributeFilterWidget.AttrState.NO);
			}
		}
	}
}
