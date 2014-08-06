using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/*
Source code copyrighgt 2014, by Michael Billard (Angel-125)
License: CC BY-NC-SA 4.0
License URL: https://creativecommons.org/licenses/by-nc-sa/4.0/
Wild Blue Industries is trademarked by Michael Billard and may be used for non-commercial purposes. All other rights reserved.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace WildBlueIndustries
{
    public class ExtendedPartModule : PartModule
    {
        [KSPField(isPersistant = true)]
        protected bool enableLogging;

        //Nodes found in the part file's MODULE config node
        //These aren't availble after the first time the part is loaded.
        protected ConfigNode.ConfigNodeList _subNodes;

        #region Logging
        public virtual void Log(object message)
        {
//            if (!enableLogging)
//                return;

            Debug.Log(this.ClassName +  " [" + this.GetInstanceID().ToString("X")
                + "][" + Time.time.ToString("0.0000") + "]: " + message);
        }

        public virtual void Log(object message, UnityEngine.Object context = null)
        {
            if (!enableLogging)
                return;

            Debug.Log(this.ClassName + " [" + this.GetInstanceID().ToString("X")
                + "][" + Time.time.ToString("0.0000") + "]: " + message, context);
        }
        #endregion

        #region Overrides

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            try
            {
                //When the part is loaded for the first time as the game starts up, we'll be reading the MODULE config node in the part's config file.
                //At that point we'll have access to sub-nodes defined in the MODULE node. Later on when the part is loaded, the game doesn't load the MODULE config node.
                //Instead, we seem to load an instance of the part.
                //Let's make a copy of the nodes and load them up when the part is instanced.
                ConfigNode.ConfigNodeList subNodes = node.nodes;
                string partTypeFilePath = getPartTypeFilePath();

                //If we have subnodes, then create the type file
                if (subNodes != null && HighLogic.LoadedScene == GameScenes.LOADING)
                {
                    ConfigNode partTypeSaveNode = new ConfigNode();

                    //Add the sub nodes
                    foreach (ConfigNode subNode in subNodes)
                        partTypeSaveNode.AddNode(subNode);

                    //Delete any existing file
                    if (File.Exists<MultiFuelSwitcher>(partTypeFilePath))
                        File.Delete<MultiFuelSwitcher>(partTypeFilePath);

                    partTypeSaveNode.Save(IOUtils.GetFilePathFor(this.GetType(), partTypeFilePath));
                }
            }

            catch (Exception ex)
            {
                Log("OnLoad generated an exception: " + ex);
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            try
            {
                string partTypeFilePath = IOUtils.GetFilePathFor(this.GetType(), getPartTypeFilePath());

                //Determine full path to the part nodes file
                if (partTypeFilePath == null)
                    return;

                //If the file exists, then we can start loading the propellant nodes.
                if (File.Exists<MultiFuelSwitcher>(partTypeFilePath))
                {
                    //Get the base node from the file
                    ConfigNode partTypeLoadNode = ConfigNode.Load(partTypeFilePath);
                    if (partTypeLoadNode == null)
                        return;

                    //Grab the sub nodes
                    _subNodes = partTypeLoadNode.nodes;
                }
            }

            catch (Exception ex)
            {
                Log("OnStart generated an exception: " + ex);
            }
        }

        #endregion

        #region Helpers
        protected string getPartTypeFilePath()
        {
            string partName = this.part.name;

            //Account for Editor
            if (HighLogic.LoadedScene == GameScenes.EDITOR)
                partName = partName.Replace("(Clone)", "");

            //Strip out invalid characters
            partName = string.Join("_", partName.Split(System.IO.Path.GetInvalidFileNameChars()));

            //Add file suffix
            return partName + "_SubNodes.cfg";
        }

        protected void showOnlyEmittersInList(List<string> emittersToShow)
        {
            KSPParticleEmitter[] emitters = part.GetComponentsInChildren<KSPParticleEmitter>();

            if (emitters == null)
                return;

            foreach (KSPParticleEmitter emitter in emitters)
            {
                //If the emitter is on the list then show it
                if (emittersToShow.Contains(emitter.name))
                {
                    emitter.emit = true;
                    emitter.enabled = true;
                }

                //Emitter is not on the list, hide it.
                else
                {
                    emitter.emit = false;
                    emitter.enabled = false;
                }
            }
        }

        protected void hideEmittersInList(List<string> emittersToHide)
        {
            KSPParticleEmitter[] emitters = part.GetComponentsInChildren<KSPParticleEmitter>();

            if (emitters == null)
                return;

            foreach (KSPParticleEmitter emitter in emitters)
            {
                //If the emitter is on the list then hide it
                if (emittersToHide.Contains(emitter.name))
                {
                    emitter.emit = false;
                    emitter.enabled = false;
                }
            }
        }

        protected void showAndHideEmitters(List<string> emittersToShow, List<string> emittersToHide)
        {
            KSPParticleEmitter[] emitters = part.GetComponentsInChildren<KSPParticleEmitter>();

            if (emitters == null)
                return;

            foreach (KSPParticleEmitter emitter in emitters)
            {
                //If the emitter is on the show list then show it
                if (emittersToShow.Contains(emitter.name))
                {
                    emitter.emit = true;
                    emitter.enabled = true;
                }

                //Emitter is on the hide list, then hide it.
                else if (emittersToHide.Contains(emitter.name))
                {
                    emitter.emit = false;
                    emitter.enabled = false;
                }
            }
        }

        #endregion
    }
}
