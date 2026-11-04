$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -Path (Join-Path $PSScriptRoot 'FurMasks.cs')
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$resources=Join-Path $project 'Assets/Capybara67/Resources'
$catalog=Get-Content (Join-Path $resources 'CompleteSets/Catalog.json') -Raw | ConvertFrom-Json
$folder=Join-Path $resources 'CompleteSets/Masks'
New-Item -ItemType Directory -Force $folder | Out-Null
$report=@()
foreach($entry in $catalog.outfits){
 $crops=[int[][]]@($entry.stages|ForEach-Object {,[int[]]@($_.crop.x,$_.crop.y,$_.crop.width,$_.crop.height)})
 $eyes=[double[][]]@($entry.stages|ForEach-Object {
   if($_.eyes.Count){,[double[]]@($_.eyes[-1].x,$_.eyes[-1].y)}else{,[double[]]@()}
 })
 $slug=$entry.resource.Split('/')[-1]
 $source=Join-Path $resources ($entry.resource+'.png')
 $bmp=[Drawing.Bitmap]::new($source)
 try{
  $data=$bmp.LockBits([Drawing.Rectangle]::new(0,0,$bmp.Width,$bmp.Height),[Drawing.Imaging.ImageLockMode]::ReadOnly,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $pixels=[byte[]]::new($data.Stride*$bmp.Height);[Runtime.InteropServices.Marshal]::Copy($data.Scan0,$pixels,0,$pixels.Length);$stride=$data.Stride;$bmp.UnlockBits($data)
  $report+=[FurMasks]::Build($source,(Join-Path $folder ($slug+'.bytes')),$pixels,$bmp.Width,$bmp.Height,$stride,$crops,$eyes)
 }finally{$bmp.Dispose()}
}
$report|Set-Content (Join-Path $PSScriptRoot 'fur-mask-review.txt')
"Built $($report.Count) shader masks. Original PNG assets unchanged."
