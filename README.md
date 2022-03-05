# 2DFieldOfView
implement the foa for 2Dgame
## Tuturiol
FOA:  
1.use ray cast to judge obstacle and record the hit point  
2.use mesh renderer to render FOA  

Shader:  
1.set all pixel's stencil value = 1  
2.set pixel's stencil value where in FOV  = 0  
3.render the gray where stencil value = 1  
4.render the target (like other players) where stencil value = 0  

## Now Problem:  
1.the gray just use alpha blend , so red just become dark red  
I've tried to implement real gray--write render feature and reder pass to grab screen image to post-processing
in urp. but when render the grey area , **all pixel's stencil value become 0** somehow.  
(the "render pass event" is"after rendering transparents")  
You can test this in scene "Postprocessing"(by change the material "StencilGrayPP" MaskId)
