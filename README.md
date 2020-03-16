# OpenVisSim: a Unity3D, data-driven sight-loss simulator, for for Virtual and Augmented Reality (VR/AR)

### About
OpenVisSim is a free library for simulating many of the symptoms associated with common eye diseases (e.g., glaucoma, AMD, diabetic retinopahty, etc.). The software is written primarily in OpenGL shaders, and supports most commercial VR/AR hardware (e.g., the HTC Vive, Google Cardboard, etc.).  Eye-tracking is also supported and is HIGHLY recommended. Until eye-tracking hardware is more widely available I will not be releasing an 'app' version of this software on iTunes or Google Play. OpenVisSIm is not suitable for simulating refractive error (i.e., long- or short-sightedness). For that, just try wearing the wrong glasses =)

### Quick Start: Setting up
Download the project. Open the demo scene "Demo1_Fove\MainScene.unity" in the Unity Editor, and click run (NB: doesn't require any hardware to be connected, but will mirror to a Fove0 headset if one is connected). When running in the editor, the mouse is used by default to simulate the observer's gaze. Filters can be modified and turned on and off by toggling the post-processing effects attached to each eye.

**Further details:**
When effects are linked, the right eye automatically copies the parameters from the left eye. A second demo is also included, designed to use the HTC Vive Eye with ZEDm cameras for AR.

### System Requirements
**Operating system:**
Any system that supports Unity3D. Can export to most VR/Ar hardware. So far we have successfully tested it with the Tobii HTC Vive, Fove0, and iPhone 7 (w/ google cardboard).

**Programming language:**
Unity 2017.4.1f1

**Dependencies:**
None (?)

**License:**
GNU GPL v3.0

### To cite and for more info
[Jones, P. R. and Ometto, G. (2018). Degraded Reality: Using VR/AR to simulate visual impairments, Proceedings of 2018 IEEE Workshop on Augmented and Virtual Realities for Good (VAR4Good), Reutlingen, Germany., pp. 1-4. doi:[10.1109/VAR4GOOD.2018.8576885]](https://www.ucl.ac.uk/~smgxprj/pdfs/jones_IEEE_2018)

[Jones, P. R., Somoskeöy, T., Chow-Wing-Bom, H., & Crabb, D. P. (2020). Seeing other perspectives: Evaluating the use of virtual and augmented reality to simulate visual impairments (OpenVisSim), NPJ Digital Medicine, 3, 32. doi:[10.1038/s41746-020-0242-6]](https://www.nature.com/articles/s41746-020-0242-6)

### Interactive WebGL Demo (early prototype version of OpenVisSim)
[https://www.ucl.ac.uk/~smgxprj/vr/](https://www.ucl.ac.uk/~smgxprj/vr/)

### Video of OpenVisSim in action
[![OpenVisSim in action](http://img.youtube.com/vi/LEGkGHwb_Fw/0.jpg)](http://www.youtube.com/watch?v=LEGkGHwb_Fw "OpenVisSim in action")

### Example of use
[Chow-Wing-Bom, H., Dekker, T. M., Jones, P. R. (2020). The worse eye revisited: Evaluating the impact of asymmetric peripheral vision loss on everyday function, Vision Research, 169, 49-57. doi:[10.1016/j.visres.2019.10.012]](https://www.sciencedirect.com/science/article/abs/pii/S0042698920300304)

### Acknowledgments
Development of the simulator was supported by Moorfields Eye Charity (#R170003A) and by the NIHR Biomedical Research Centre located at (both) Moorfields Eye Hospital and UCL Institute of Ophthalmology.

### Contact me
For any questions/comments, feel free to email me at: peter.jones@city.ac.uk

### Enjoy!
@petejonze  
16/03/2020