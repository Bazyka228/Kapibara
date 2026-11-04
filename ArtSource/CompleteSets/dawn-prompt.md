# Хранитель рассвета — награда календаря

Built-in image_gen, transparent background, 20 October 2026. Reference: mandarin-gardener-sample.png. Final game asset: Assets/Capybara67/Resources/CompleteSets/dawn.png.

## Initial prompt

Use case: stylized-concept. Production transparent sprite atlas for a Unity 2D capybara game. Reference image is style and five age poses only. Make ONE new complete outfit called Dawn Guardian: elegant ivory short cape with turquoise satin lining, intricate gold embroidered sun rays and small botanical patterns, rounded sunrise medallion, pearl clasps, teal waistcoat, refined small golden sun diadem fitted directly on the head (not floating). Same friendly rounded capybaras, warm caramel fur, thick dark brown clean outlines, soft painted highlights, delicate fabric seams, pink cheeks and simple dot pupils. Exactly FIVE separate full-body capybaras, three across upper row and two across lower row, generously separated with absolutely no overlapping crop rectangles. Age1 reclining newborn eyes closed; age2 small seated baby with two dot pupils; age3 curious upright youngster facing right with one dot pupil; age4 larger seated young adult with one dot pupil; age5 majestic yet cute guardian with one dot pupil. Match reference morphology and readable brown silhouette, natural paws. Every character wears fitted headwear AND complete clothing. Keep each whole silhouette including crown and cape inside its cell with clear transparent margins. No separate sparkles, particles, labels, text, frame, ground, shadow or background. True transparent alpha.

## Final layout revision

Preserve all five characters, exact clothing designs, style, colors, expressions and true transparent background. Add a clear 70-pixel horizontal transparent gutter between upper three characters and bottom two characters so rectangular bounding boxes do not overlap. Scale slightly smaller within cells if needed. Keep each complete sprite with transparent margin. No detached particles.

## Validation

Build-Catalog.ps1 -RequireAll: 36 sets, 180 portraits. All rectangular crops disjoint. Newborn and young-adult have painted closed eyes; baby has two pupils; curious and guardian have one. Only those pupils animate.
