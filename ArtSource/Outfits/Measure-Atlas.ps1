param([Parameter(Mandatory=$true)][string]$Path,[switch]$Eyes,[ValidateSet(5,9)][int]$ExpectedCount=9)
$ErrorActionPreference='Stop'
# Read-only alpha bounds measurement. Never modifies artwork.
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
public static class OutfitBounds {
 public static int[][] DarkFeatures(byte[] bytes,int width,int height,int stride) {
  var seen=new bool[width*height];var queue=new int[width*height];
  var found=new System.Collections.Generic.List<int[]>();
  for(int y=0;y<height;y++)for(int x=0;x<width;x++) {
   int start=y*width+x,offset=y*stride+x*4;
   if(seen[start]||bytes[offset+3]<200||bytes[offset+2]>120||bytes[offset+1]>70||bytes[offset]>50)continue;
   int count=0,end=1,left=x,right=x,top=y,bottom=y;queue[0]=start;seen[start]=true;
   while(count<end){int current=queue[count++],cx=current%width,cy=current/width;left=Math.Min(left,cx);right=Math.Max(right,cx);top=Math.Min(top,cy);bottom=Math.Max(bottom,cy);
    foreach(int next in new[]{current-1,current+1,current-width,current+width}){
     if(next<0||next>=seen.Length||seen[next])continue;int nx=next%width,ny=next/width;if(Math.Abs(nx-cx)+Math.Abs(ny-cy)!=1)continue;
     int p=ny*stride+nx*4;if(bytes[p+3]<200||bytes[p+2]>120||bytes[p+1]>70||bytes[p]>50)continue;
     seen[next]=true;queue[end++]=next;
    }
   }
   if(count>35&&count<900&&right-left<45&&bottom-top<40)found.Add(new[]{(left+right)/2,(top+bottom)/2,right-left+1,bottom-top+1,count});
  }
  return found.ToArray();
 }
 public static int[][] Read(byte[] bytes,int width,int height,int stride,int expected) {
    var seen=new bool[width*height];var queue=new int[width*height];
    var found=new System.Collections.Generic.List<int[]>();
    for(int y=0;y<height;y++)for(int x=0;x<width;x++) {
     int start=y*width+x;if(seen[start]||bytes[y*stride+x*4+3]<24)continue;
     int count=0,end=1,left=x,right=x,top=y,bottom=y;queue[0]=start;seen[start]=true;
     while(count<end){int current=queue[count++],cx=current%width,cy=current/width;left=Math.Min(left,cx);right=Math.Max(right,cx);top=Math.Min(top,cy);bottom=Math.Max(bottom,cy);
      foreach(int next in new[]{current-1,current+1,current-width,current+width}){
       if(next<0||next>=seen.Length||seen[next])continue;int nx=next%width,ny=next/width;if(Math.Abs(nx-cx)+Math.Abs(ny-cy)!=1||bytes[ny*stride+nx*4+3]<24)continue;
       seen[next]=true;queue[end++]=next;
      }
     }
     if(count<10000)continue;
     found.Add(new[]{left,top,right-left+1,bottom-top+1,count});
    }
    if(found.Count!=expected)throw new Exception("Expected "+expected+" separate silhouettes, got "+found.Count);
    int rows=expected==5?2:3;
    found.Sort((a,b)=>{int rowA=Math.Min(rows-1,(a[1]*2+a[3])*rows/(2*height)),rowB=Math.Min(rows-1,(b[1]*2+b[3])*rows/(2*height));return rowA==rowB?a[0].CompareTo(b[0]):rowA.CompareTo(rowB);});
    return found.ToArray();
 }
}
'@
$atlas=Get-Item -LiteralPath $Path
$bitmap=[System.Drawing.Bitmap]::new($atlas.FullName)
try {
 $data=$bitmap.LockBits([System.Drawing.Rectangle]::new(0,0,$bitmap.Width,$bitmap.Height),[System.Drawing.Imaging.ImageLockMode]::ReadOnly,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
 try {
  $bytes=[byte[]]::new($data.Stride*$data.Height)
  [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0,$bytes,0,$bytes.Length)
  $bounds=[OutfitBounds]::Read($bytes,$bitmap.Width,$bitmap.Height,$data.Stride,$ExpectedCount)
  if($Eyes){$features=[OutfitBounds]::DarkFeatures($bytes,$bitmap.Width,$bitmap.Height,$data.Stride)}
 } finally { $bitmap.UnlockBits($data) }
} finally { $bitmap.Dispose() }
@{file=$atlas.Name;bounds=@($bounds|ForEach-Object {,@($_)})}|ConvertTo-Json -Depth 5 -Compress
if($Eyes){@{file=$atlas.Name;features=@($features|ForEach-Object {,@($_)})}|ConvertTo-Json -Depth 5 -Compress}
