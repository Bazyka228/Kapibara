using System;
using System.Collections.Generic;
using System.IO;
// Produces a semantic selection for the shader; source illustrations remain unchanged.
public static class FurMasks {
 sealed class Region {
  public List<int> pixels=new List<int>();
  public double r,g,b;public int minX=int.MaxValue,minY=int.MaxValue,maxX,maxY;
 }
 static bool Fur(byte[] p,int at){
  int b=p[at],g=p[at+1],r=p[at+2];
  return p[at+3]>220&&r>165&&g>103&&b>55&&r-g>24&&r-g<101&&g-b>20&&g-b<106;
 }
 public static string Build(string source,string output,byte[] p,int w,int h,int stride,int[][] crops,double[][] eyes){
   var mask=new bool[w*h];var summaries=new List<string>();
   for(int age=0;age<crops.Length;age++){
    var c=crops[age];int x0=c[0],y0=c[1],cw=c[2],ch=c[3];var seen=new bool[cw*ch];var all=new List<Region>();
    for(int yy=0;yy<ch;yy++)for(int xx=0;xx<cw;xx++){
     int start=yy*cw+xx;if(seen[start]||!Fur(p,(y0+yy)*stride+(x0+xx)*4))continue;
     var reg=new Region();var q=new Queue<int>();q.Enqueue(start);seen[start]=true;
     while(q.Count>0){
      int n=q.Dequeue(),x=n%cw,y=n/cw,px=x+x0,py=y+y0,at=py*stride+px*4;
      reg.pixels.Add(py*w+px);reg.r+=p[at+2];reg.g+=p[at+1];reg.b+=p[at];
      reg.minX=Math.Min(reg.minX,px);reg.maxX=Math.Max(reg.maxX,px);reg.minY=Math.Min(reg.minY,py);reg.maxY=Math.Max(reg.maxY,py);
      foreach(int next in new[]{n-1,n+1,n-cw,n+cw}){
       if(next<0||next>=seen.Length||seen[next])continue;int nx=next%cw,ny=next/cw;
       if(Math.Abs(nx-x)+Math.Abs(ny-y)!=1||!Fur(p,(y0+ny)*stride+(x0+nx)*4))continue;
       seen[next]=true;q.Enqueue(next);
      }
     }
     if(reg.pixels.Count<40)continue;
     reg.r/=reg.pixels.Count;reg.g/=reg.pixels.Count;reg.b/=reg.pixels.Count;all.Add(reg);
    }
    double tx=x0+cw*(age==0?.67:.64),ty=y0+ch*(age==0?.57:.38);
    if(eyes[age].Length>=2){tx=eyes[age][0];ty=eyes[age][1]+20;}
    Region face=null;double best=double.MaxValue;
    foreach(var r in all){
     if(r.pixels.Count<cw*ch*.008)continue;
     double distance=double.MaxValue;
     foreach(int n in r.pixels){double dx=n%w-tx,dy=n/w-ty;distance=Math.Min(distance,dx*dx+dy*dy);}
     if(distance<best){best=distance;face=r;}
    }
    if(face==null||best>cw*ch*.035)throw new Exception("No reliable face selection: "+source+" age "+(age+1));
    var chosen=new List<Region>{face};
    foreach(var r in all){
     if(r==face)continue;
     // Exposed wrists and ankles match face pigments; clothing ornaments are too small.
     bool feet=r.minY>y0+ch*.63&&r.maxX>x0+cw*.43&&r.pixels.Count>cw*ch*.003&&r.pixels.Count<cw*ch*.13;
     double delta=Math.Abs(r.r-face.r)+Math.Abs(r.g-face.g)+Math.Abs(r.b-face.b);
     // These short jackets expose a large rear flank. Keep this semantic override
     // explicit: peach trousers in other sets can match the face pigments.
     string slug=Path.GetFileNameWithoutExtension(source);
     bool shortJacket=Array.IndexOf(new[]{"forest","winter","musician","detective","tea","festival","librarian","aviator"},slug)>=0;
     bool rear=shortJacket&&r.minY>y0+ch*.55&&r.maxX<x0+cw*.6&&r.pixels.Count>cw*ch*.003&&r.pixels.Count<cw*ch*.13;
     bool arm=slug=="marine"&&r.minY>y0+ch*.43&&r.maxX>x0+cw*.5&&r.pixels.Count>cw*ch*.008&&r.pixels.Count<cw*ch*.15;
     if((feet||rear||arm)&&delta<43)chosen.Add(r);
    }
    int count=0;foreach(var r in chosen)foreach(int n in r.pixels){mask[n]=true;count++;}
    // Fill enclosed ink holes in the face. The shader still protects pupils/cheeks,
    // but fur reconstructed under a blinking pupil must receive the same colour.
    int fx=Math.Max(0,face.minX-1),fy=Math.Max(0,face.minY-1),fw=Math.Min(w-1,face.maxX+1)-fx+1,fh=Math.Min(h-1,face.maxY+1)-fy+1;
    var outside=new bool[fw*fh];var flood=new Queue<int>();
    for(int n=0;n<outside.Length;n++){
     int x=n%fw,y=n/fw;if(x!=0&&y!=0&&x!=fw-1&&y!=fh-1)continue;
     if(!mask[(fy+y)*w+fx+x]){outside[n]=true;flood.Enqueue(n);}
    }
    while(flood.Count>0){
     int n=flood.Dequeue(),x=n%fw,y=n/fw;
     foreach(int next in new[]{n-1,n+1,n-fw,n+fw}){
      if(next<0||next>=outside.Length||outside[next])continue;int nx=next%fw,ny=next/fw;
      if(Math.Abs(nx-x)+Math.Abs(ny-y)!=1||mask[(fy+ny)*w+fx+nx])continue;
      outside[next]=true;flood.Enqueue(next);
     }
    }
    for(int n=0;n<outside.Length;n++)if(!outside[n])mask[(fy+n/fw)*w+fx+n%fw]=true;
    summaries.Add((age+1)+":"+count+":"+chosen.Count);
   }
   // Horizontal runs keep the resource small and avoid needing readable source textures at runtime.
   using(var writer=new BinaryWriter(File.Create(output))){
    writer.Write(0x43504631);writer.Write(w);writer.Write(h);
    int start=-1;
    for(int n=0;n<=mask.Length;n++){
     if(n<mask.Length&&mask[n]){if(start<0)start=n;}
     else if(start>=0){writer.Write(start);writer.Write(n-start);start=-1;}
    }
   }
   return Path.GetFileName(source)+" "+String.Join(" ",summaries);
 }
}
