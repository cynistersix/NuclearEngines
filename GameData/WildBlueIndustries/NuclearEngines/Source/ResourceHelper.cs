using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/*
Source code copyrighgt 2014, by Michael Billard (Angel-125)
License: CC BY-NC-SA 4.0
License URL: https://creativecommons.org/licenses/by-nc-sa/4.0/
Wild Blue Industries is trademarked by Michael Billard and may be used for non-commercial purposes. All other rights reserved.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace WildBlueIndustries
{
    class ResourceHelper
    {
        public static double ConsumeResource(List<PartResource> resources, double amountRequested)
        {
            double amountAcquired = 0;
            double amountRemaining = amountRequested;

            foreach (PartResource resource in resources)
            {
                //Do we have more than enough?
                if (resource.amount >= amountRemaining)
                {
                    //We got what we wanted, yay. :)
                    amountAcquired += amountRemaining;

                    //reduce the part resource's current amount
                    resource.amount -= amountRemaining;

                    //Done
                    break;
                }

                //PartResource's amount < amountRemaining
                //Drain the resource dry
                else
                {
                    amountAcquired += resource.amount;

                    resource.amount = 0;
                }
            }

            return amountAcquired;
        }

        public static bool VesselHasResource(string resourceName, Vessel vessel)
        {
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            List<PartResource> resources;
            List<Part> parts;
            int resourceID;

            //First, does the resource definition exist?
            if (definitions.Contains(resourceName))
            {
                resources = new List<PartResource>();
                resourceID = definitions[resourceName].id;

                //The definition exists, now see if the vessel has the resource
                parts = vessel.parts;
                foreach (Part part in parts)
                {
                    part.GetConnectedResources(resourceID, ResourceFlowMode.NULL, resources);

                    //If somebody has the resource, then we're good.
                    if (resources.Count > 0)
                        return true;
                }
            }

            return false;
        }

        public static List<PartResource> GetConnectedResources(string resourceName, Part part)
        {
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            List<PartResource> resources = new List<PartResource>();
            int resourceID;

            if (definitions.Contains(resourceName))
            {
                resourceID = definitions[resourceName].id;
                part.GetConnectedResources(resourceID, ResourceFlowMode.NULL, resources);
            }

            return resources;
        }

        public static List<PartResource> GetConnectedResources(int resourceID, Part part)
        {
            List<PartResource> resources = new List<PartResource>();

            part.GetConnectedResources(resourceID, ResourceFlowMode.NULL, resources);

            return resources;
        }

        public static float CapacityRemaining(List<PartResource> resources)
        {
            float capacityRemaining = 0;

            foreach (PartResource resource in resources)
                capacityRemaining += (float)(resource.maxAmount - resource.amount);

            return capacityRemaining;
        }

        public static float DistributeResource(List<PartResource> resources, float amount)
        {
            float remainingAmount = amount;
            float amountPerContainer = amount / resources.Count;

            foreach (PartResource resource in resources)
            {
                //Does the resource container have enough room?
                if ((resource.maxAmount - resource.amount) >= amountPerContainer)
                {
                    resource.amount += amountPerContainer;
                    remainingAmount -= amountPerContainer;
                }
            }

            return remainingAmount;
        }

        public static float DistributeResource(string resourceName, float amount, Part part)
        {
            List<PartResource> resources = GetConnectedResources(resourceName, part);

            if (resources == null)
                return amount;

            return DistributeResource(resources, amount);
        }
    }
}
