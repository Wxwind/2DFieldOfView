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

## To improve:  
1.the gray just use alpha blend , so red just become dark red  
if want real gray,you can use post processing in urp  (need to write render feature and reder pass).
