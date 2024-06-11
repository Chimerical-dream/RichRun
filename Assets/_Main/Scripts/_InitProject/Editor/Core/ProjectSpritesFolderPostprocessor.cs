using UnityEditor;

/// <summary>
/// Changes all images to Sprite type if it's located in Sprites
/// </summary>
class ProjectSpritesFolderPostprocessor : AssetPostprocessor {

	private const string projectMainPath = "Assets/_Main";
	private const string projectSpritesFolderPath = "/Sprites";

	// texture asset preprocessor
	void OnPreprocessTexture() {
		if (assetPath.StartsWith(projectMainPath + projectSpritesFolderPath)) {
			PreprocessSpriteSource();
		}
	}

	void PreprocessSpriteSource() {
		TextureImporter importer = assetImporter as TextureImporter;
		if (importer) {
			importer.textureType = TextureImporterType.Sprite;
		}
	}
}