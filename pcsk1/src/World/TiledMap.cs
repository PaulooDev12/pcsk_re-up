using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System;
using TiledSharp;
using System.Diagnostics;

namespace pcsk1.src.World
{
    /// <summary>
    /// Representa um mapa criado no Tiled e fornece os recursos necessários
    /// para carregá-lo, desenhá-lo e consultar suas áreas de colisão.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A classe utiliza <see cref="TmxMap"/> para interpretar o arquivo XML
    /// do mapa. As imagens dos tilesets são carregadas diretamente do sistema
    /// de arquivos com <see cref="File.OpenRead(string)"/> e convertidas em
    /// <see cref="Texture2D"/> por <see cref="Texture2D.FromStream"/>.
    /// Portanto, o mapa e as imagens referenciadas precisam estar disponíveis
    /// no caminho físico esperado no momento da construção do objeto.
    /// </para>
    /// <para>
    /// As colisões podem ser definidas de duas formas: pela propriedade
    /// booleana <c>solid</c> de um tile no tileset ou por objetos desenhados no
    /// grupo de objetos chamado <c>Collisions</c>. As duas fontes são somadas
    /// na coleção exposta por <see cref="CollisionsRect"/>.
    /// </para>
    /// <para>
    /// Esta classe mantém as texturas carregadas durante toda a sua vida útil.
    /// O chamador deve garantir que o mapa seja utilizado no mesmo contexto
    /// gráfico do <see cref="GraphicsDevice"/> recebido no construtor e que o
    /// objeto não seja usado antes de sua construção terminar.
    /// </para>
    /// </remarks>
    public class TiledMap
    {
        /// <summary>
        /// Nome da propriedade booleana usada para marcar tiles sólidos.
        /// A comparação do nome não diferencia maiúsculas de minúsculas.
        /// </summary>
        private const string SolidProperty = "solid";

        /// <summary>
        /// Nome do grupo de objetos do Tiled que contém retângulos de colisão.
        /// </summary>
        private const string CollisionsObjectGroup = "Collisions";

        /// <summary>
        /// Estrutura do mapa carregada pelo TiledSharp.
        /// </summary>
        private TmxMap _map;

        /// <summary>
        /// Largura, em pixels, de cada tile do mapa.
        /// </summary>
        private readonly int _tileWidth;

        /// <summary>
        /// Altura, em pixels, de cada tile do mapa.
        /// </summary>
        private readonly int _tileHeight;

        /// <summary>
        /// Retângulos de colisão gerados a partir dos tiles sólidos e do grupo
        /// de objetos de colisão.
        /// </summary>
        private readonly List<Rectangle> _collisions = new();

        /// <summary>
        /// Texturas carregadas para os tilesets, indexadas pelo <c>FirstGid</c>
        /// de cada tileset.
        /// </summary>
        private readonly Dictionary<int, Texture2D> _tilesetTextures = new();

        /// <summary>
        /// Obtém os retângulos de colisão calculados para o mapa.
        /// </summary>
        /// <remarks>
        /// A coleção é somente leitura para o consumidor, mas é mantida
        /// internamente pela classe. Ela não deve ser modificada diretamente.
        /// Os retângulos usam coordenadas do mundo, em pixels, e não
        /// coordenadas de índice de tile.
        /// </remarks>
        /// <value>Lista somente leitura com os retângulos de colisão.</value>
        public IReadOnlyList<Rectangle> CollisionsRect => _collisions; 

        /// <summary>
        /// Carrega um mapa do Tiled, suas texturas e suas colisões.
        /// </summary>
        /// <param name="graphicsDevice">
        /// Dispositivo gráfico usado para criar as texturas dos tilesets.
        /// </param>
        /// <param name="mapPath">
        /// Caminho do arquivo <c>.tmx</c> do mapa. O diretório desse caminho é
        /// usado como base para localizar as imagens dos tilesets.
        /// </param>
        /// <param name="content">
        /// Gerenciador de conteúdo recebido pela API atual. Atualmente esse
        /// parâmetro não é utilizado: as imagens são abertas diretamente por
        /// caminho físico em vez de serem carregadas pelo Content Pipeline.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Não é lançada explicitamente nesta implementação para argumentos
        /// nulos; a biblioteca ou as operações subsequentes podem falhar caso
        /// um argumento obrigatório seja nulo.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Pode ocorrer quando o arquivo do mapa ou uma imagem não existe.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Pode ocorrer quando algum diretório do caminho do mapa ou de uma
        /// imagem não existe.
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// Pode ocorrer quando o arquivo não representa um mapa Tiled válido.
        /// </exception>
        public TiledMap(GraphicsDevice graphicsDevice, string mapPath, ContentManager content)
        {
            _map = new TmxMap(mapPath);
            _tileWidth = _map.TileWidth;
            _tileHeight = _map.TileHeight;

            string mapDirectory = Path.GetDirectoryName(mapPath) ?? "Content/maps";

            foreach (var tileset in _map.Tilesets)
            {
                if (tileset.Image == null) continue;

                string imageFileName = Path.GetFileName(tileset.Image.Source);
                string imagePath = Path.Combine(mapDirectory, imageFileName);
                using (var stream = File.OpenRead(imagePath))
                {
                    Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
                    _tilesetTextures.Add(tileset.FirstGid, texture);
                }
            }
            GenerateCollisions();
        }

        /// <summary>
        /// Obtém o tileset responsável por um GID global do mapa.
        /// </summary>
        /// <param name="gid">GID global do tile no mapa.</param>
        /// <returns>
        /// O tileset cujo intervalo começa em seu <c>FirstGid</c>; retorna
        /// <see langword="null"/> quando nenhum tileset corresponde ao GID.
        /// </returns>
        /// <remarks>
        /// O GID zero representa uma posição vazia e normalmente não deve ser
        /// consultado por este método. A implementação depende da ordem dos
        /// tilesets em <see cref="TmxMap.Tilesets"/> e do número de texturas
        /// carregadas.
        /// </remarks>
        private TmxTileset GetTilesetForGid(int gid)
        {
            for (int i = _tilesetTextures.Count - 1; i >= 0; i--)
            {
                if(gid >= _map.Tilesets[i].FirstGid) return _map.Tilesets[i];
            }
            return null;
        }

        /// <summary>
        /// Reconstrói toda a lista de colisões do mapa.
        /// </summary>
        /// <remarks>
        /// A lista é limpa antes de ser preenchida. Primeiro são processados
        /// os tiles marcados como sólidos e depois os objetos do grupo
        /// <c>Collisions</c>. Se as duas fontes descrevem a mesma área, dois
        /// retângulos serão adicionados; não há remoção ou união automática.
        /// </remarks>
        private void GenerateCollisions()
        {
            _collisions.Clear();
            AddCollisionsFromSolidTiles();
            AddCollisionsFromGroup();
        }

        /// <summary>
        /// Adiciona uma colisão para cada tile com a propriedade <c>solid</c>
        /// avaliada como <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// O retângulo é calculado multiplicando a posição do tile pela largura
        /// e pela altura do tile do mapa. A implementação atual utiliza
        /// <c>_tileWidth</c> para a coordenada vertical e para a altura do
        /// retângulo; mapas com tiles não quadrados podem exigir revisão desse
        /// comportamento.
        /// </remarks>
        private void AddCollisionsFromSolidTiles()
        {
            foreach(var layer in _map.Layers)
            {
                foreach(var tile in layer.Tiles)
                {
                    if (tile.Gid == 0) continue;
                    var tileset = GetTilesetForGid(tile.Gid);
                    if (tileset == null) continue;

                    int frame = tile.Gid - tileset.FirstGid;

                    if (!IsSolid(tileset, frame)) continue;
                    _collisions.Add(new Rectangle(
                        tile.X * _tileWidth, 
                        tile.Y * _tileWidth, 
                        _tileWidth, _tileWidth));
                }
            }
        }

        /// <summary>
        /// Adiciona retângulos definidos no grupo de objetos <c>Collisions</c>.
        /// </summary>
        /// <remarks>
        /// As coordenadas e dimensões dos objetos são convertidas de ponto
        /// flutuante para inteiro por truncamento. Se o grupo não existir, o
        /// método não altera a lista de colisões.
        /// </remarks>
        private void AddCollisionsFromGroup()
        {
            if (!_map.ObjectGroups.Contains(CollisionsObjectGroup)) return;
            foreach(var obj in _map.ObjectGroups[CollisionsObjectGroup].Objects)
            {
                _collisions.Add(
                    new Rectangle(
                        (int)obj.X,
                        (int)obj.Y,
                        (int)obj.Width,
                        (int)obj.Height
                        )
                    );
            }
        }

        /// <summary>
        /// Verifica se um tile do tileset está marcado como sólido.
        /// </summary>
        /// <param name="tileset">Tileset que contém o tile.</param>
        /// <param name="frame">
        /// Índice local do tile dentro do tileset, calculado a partir do GID.
        /// </param>
        /// <returns>
        /// <see langword="true"/> quando o tile possui a propriedade
        /// <c>solid</c> com valor booleano verdadeiro; caso contrário,
        /// <see langword="false"/>.
        /// </returns>
        private static bool IsSolid(TmxTileset tileset, int frame)
        {
            if (!tileset.Tiles.TryGetValue(frame, out var tilesetTile)) return false;

            foreach(var property in tilesetTile.Properties)
            {
                bool isSolidProperty = string.Equals(property.Key, SolidProperty, StringComparison.OrdinalIgnoreCase);
                if (isSolidProperty && bool.TryParse(property.Value, out bool value))
                {
 
                    return value;
                }

            }
            return false;
        }

        /// <summary>
        /// Desenha as camadas e os tiles do mapa usando o <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="spriteBatch">
        /// SpriteBatch que deve estar em um estado compatível com as chamadas
        /// de desenho durante a execução deste método.
        /// </param>
        /// <remarks>
        /// A posição de destino de cada tile é derivada de suas coordenadas e
        /// das dimensões do mapa. A opacidade da camada é aplicada como parte
        /// da tonalidade. Os efeitos de espelhamento horizontal e vertical
        /// definidos no Tiled também são convertidos para
        /// <see cref="SpriteEffects"/>.
        /// </remarks>
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach(var layer in _map.Layers)
            {
                if (layer.Visible) continue;

                Color tint = Color.White * (float)layer.Opacity;

                foreach (var tile in layer.Tiles)
                    DrawTile(spriteBatch, tile, tint);
            }
        }

        /// <summary>
        /// Desenha um tile individual, se ele possuir um GID e um tileset
        /// carregado correspondente.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch usado para desenhar o tile.</param>
        /// <param name="tile">Tile da camada que será desenhado.</param>
        /// <param name="tint">Cor e opacidade aplicadas ao tile.</param>
        private void DrawTile(SpriteBatch spriteBatch, TmxLayerTile tile, Color tint)
        {
            if (tile.Gid == 0) return;
            var tileset = GetTilesetForGid(tile.Gid);
            if (!_tilesetTextures.TryGetValue(tileset.FirstGid, out Texture2D texture)) return;

            int frame = tile.Gid - tileset.FirstGid;
            Rectangle sourceRect = GetSourceRect(tileset, texture, frame);
            Rectangle destRect = GetDestinationRect(tile);

            spriteBatch.Draw(
                texture,
                destRect,
                sourceRect,
                tint,
                0f,
                Vector2.Zero,
                GetFlipEffects(tile),
                0f
                );

        }

        /// <summary>
        /// Calcula o retângulo de destino de um tile nas coordenadas do mundo.
        /// </summary>
        /// <param name="tile">Tile cuja posição será convertida.</param>
        /// <returns>
        /// Retângulo em pixels com origem na posição do tile e dimensões iguais
        /// às dimensões dos tiles do mapa.
        /// </returns>
        private Rectangle GetDestinationRect(TmxLayerTile tile)
        {
            return new Rectangle(
                tile.X * _tileWidth,
                tile.Y * _tileHeight,
                _tileWidth,
                _tileHeight
                );
        }

        private static Rectangle GetSourceRect(TmxTileset tileset, Texture2D texture, int frame)
        {
            int columns = GetColumns(tileset, texture);
            int column = frame % columns;
            int row = frame / columns;
            return new Rectangle(
                tileset.Margin + column * (tileset.TileWidth + tileset.Spacing),
                tileset.Margin + row * (tileset.TileHeight + tileset.Spacing),
                tileset.TileWidth,
                tileset.TileHeight
                );
        }

        /// <summary>
        /// Obtém o número de colunas da imagem de um tileset.
        /// </summary>
        /// <param name="tileset">Tileset cuja configuração será consultada.</param>
        /// <param name="texture">Textura usada para inferir as colunas.</param>
        /// <returns>
        /// O número configurado no Tiled ou, quando ausente, o número calculado
        /// a partir da largura da textura, margem, espaçamento e largura do
        /// tile. O resultado mínimo é um.
        /// </returns>
        private static int GetColumns(TmxTileset tileset, Texture2D texture)
        {
            if (tileset.Columns.HasValue && tileset.Columns.Value > 0)
                return tileset.Columns.Value;

            int usableWidth = texture.Width - tileset.Margin * 2 + tileset.Spacing;
            return Math.Max(1, usableWidth / (tileset.TileWidth + tileset.Spacing));
        }

        /// <summary>
        /// Converte os indicadores de espelhamento do Tiled para efeitos do
        /// MonoGame.
        /// </summary>
        /// <param name="tile">Tile que contém os indicadores de orientação.</param>
        /// <returns>
        /// Combinação de <see cref="SpriteEffects.FlipHorizontally"/> e
        /// <see cref="SpriteEffects.FlipVertically"/>, ou
        /// <see cref="SpriteEffects.None"/> quando não há espelhamento.
        /// </returns>
        private static SpriteEffects GetFlipEffects(TmxLayerTile tile)
        {
            var effects = SpriteEffects.None;
            if (tile.HorizontalFlip) effects |= SpriteEffects.FlipHorizontally;
            if (tile.VerticalFlip) effects |= SpriteEffects.FlipVertically;
            return effects;
        }
    }
}
