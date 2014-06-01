using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3D
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]

    public class K3D : MonoBehaviour
    {
        PXCUPipeline _pipeline;
        bool _initialized;

        PXCMRectU32 _lastFacePos;
        

        void Start()
        {
            _pipeline = new PXCUPipeline();
            PXCUPipeline.Mode mode = PXCUPipeline.Mode.FACE_LANDMARK | PXCUPipeline.Mode.FACE_LOCATION | PXCUPipeline.Mode.GESTURE;
            print (mode);
			if (_pipeline.Init(mode))
            {
                print("I see you!");
                _initialized = true;
            }
            else
            {
                print("K3D Initialization failed!");
                _initialized = false;
			}
			
			bool success;
			PXCMFaceAnalysis.Detection.Data data = getDetectionData(out success);
			_lastFacePos = data.rectangle;
        }

        void Update()
        {
        	if(!_initialized){
        		return;
        	}
        	
        	if(!_pipeline.AcquireFrame(false)){
        		return;
        	}
			
			bool success;
			PXCMFaceAnalysis.Detection.Data data = getDetectionData(out success);
			int delX, delY;
			
			if( success ){
				delX = (int)_lastFacePos.x - (int)data.rectangle.x;
				delY = (int)_lastFacePos.y - (int)data.rectangle.y;
				_lastFacePos = data.rectangle;
			} else {
				delX = 0;
				delY = 0;
			}
			
            switch (CameraManager.Instance.currentCameraMode)
            {
                case CameraManager.CameraMode.Flight:
                    print("OMG I'M FLYING");
                    //FlightCamera.
                    break;

                case CameraManager.CameraMode.Internal:
				case CameraManager.CameraMode.IVA:
					Vector3 pos = InternalCamera.Instance.transform.localPosition;
					InternalCamera.Instance.transform.localPosition = new Vector3(pos.x - (delX/1000f), pos.y - (delY/1000f), pos.z);
					FlightCamera.fetch.transform.rotation = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.rotation);
					FlightCamera.fetch.transform.position = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.position);
                    break;

                case CameraManager.CameraMode.Map:
                    print("ITS A MAP");
                    //PlanetariumCamera.
                    break;
            }
            
            _pipeline.ReleaseFrame();
        }
        
        private PXCMFaceAnalysis.Detection.Data getDetectionData(out bool success)
        {
			PXCMFaceAnalysis.Detection.Data ddata = new PXCMFaceAnalysis.Detection.Data();
			success = false;
			for (int i=0;;i++) {
				int face; ulong timeStamp;
				if (!_pipeline.QueryFaceID(i,out face, out timeStamp)) break;
				//print("face "+i+" (id=" + face + ", timeStamp=" + timeStamp+")");
				
				if(_pipeline.QueryFaceLocationData(face,out ddata)){
					success = true;
				}
			}
			return ddata;
		}
    }
}
