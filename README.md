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
FieldOfView场景里参考的是youtube上的教程 [Field of view visualisation (E01)](https://www.youtube.com/watch?v=rQG9aUWarwE)    
FieldOfViewImprove场景参考教程优化[【开源】俯视角视野范围显示与优化](https://www.bilibili.com/video/BV1hS4y1S7Y3)  
 
与Collider.ClosedPoint相比，由于Collider2D.ClosedPoint返回的点不一定在碰撞体的包围盒上，因此只能进行不精确的手动矫正，具体参考代码  
