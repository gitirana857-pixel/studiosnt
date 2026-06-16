# Play Store - STUDIOS NT

## Assets
- `playstore_icon.png` — 512x512 — App icon for Play Store listing
- `playstore_icon.svg` — Vector source
- `feature_graphic.png` — 1024x500 — Feature graphic for Store listing
- `feature_graphic.svg` — Vector source

## AAB (Android App Bundle)
O AAB assinado é gerado automaticamente pelo GitHub Actions no job **Build AAB (Release)**.
Disponível nos **Releases** do GitHub com a tag `aab-v...`.

## Para publicar no Google Play Console:

1. **Acesse** https://play.google.com/console
2. **Crie um app** (se ainda não criou):
   - Nome: STUDIOS NT
   - Plataforma: Android
   - Nome do pacote: com.studiosnt.timeline
3. **App Signing by Google Play** (recomendado):
   - Na primeira publicação, faça upload do AAB
   - Google vai gerar a chave de assinatura automaticamente
   - A chave de upload (nossa) fica segura como GitHub Secret
4. **Preencha a Store Listing**:
   - Ícone: playstore_icon.png
   - Feature Graphic: feature_graphic.png
   - Screenshots: Faça screenshots reais do app no celular (2-8 capturas)
   - Descrição curta e completa (em português)
   - Categoria: Social
   - Política de privacidade: URL (criar uma página ou usar gerador)
5. **Responder ao questionário de classificação de conteúdo**
6. **Definir preço**: Gratuito
7. **Enviar para revisão** com o AAB do último Release

> ⚠️ **IMPORTANTE**: Na primeira vez que fizer upload do AAB, o Google Play Console
> vai detectar App Signing. Siga o assistente. Depois disso, sempre baixe o
> **PEM de assinatura de upload** do Play Console e salve-o com a keystore.
