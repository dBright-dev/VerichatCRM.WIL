import { CRMTheme } from '../constants/crmTheme'
import { MaterialCommunityIcons } from '@expo/vector-icons'; //insert animated icons in expo.icon
import { LinearGradient } from 'expo-linear-gradient';
import { useFocusEffect, useRouter } from 'expo-router';
import { ComponentProps, useCallback, useEffect, useMemo, useState } from 'react';
import {
  FlatList,
  Image,
  ImageSourcePropType,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { fetchMobileCatalogue, MobileProduct } from '../constants/catalogueService';
import { addCartItem, getCartLines, subscribeCart } from '../constants/cartStore';
import BarcodeScannerModal from '../components/barcode-scanner-modal';
import SkeletonCard from '../components/ui/SkeletonCard';
import Animated, { FadeInDown } from 'react-native-reanimated';
import AnimatedButton from '../components/ui/AnimatedButton';
import { subscribeGlobalRefresh } from '../utils/refreshEvents';

/* ----------------------------- Types & data ----------------------------- */

type IconName = ComponentProps<typeof MaterialCommunityIcons>['name']

type Category = string

type Product = {
  id: string
  name: string
  sku: string
  category: Category
  price: number
  unit: string // "each", "per pack", ...
  icon: IconName
  // Optional: add a real product photo, e.g. require('../../assets/images/products/stethoscope.png')
  image?: ImageSourcePropType
}

const CURRENCY = 'R'

type Filter = string


const formatPrice = (value: number) =>
  `${CURRENCY}${value.toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`

// Icons for categories served by the CRM product API.
const CATEGORY_ICONS: Record<string, IconName> = {
  PPE: 'face-mask',
  'Infant Care': 'baby-face-outline',
  Hospital: 'hospital-building',
  Lab: 'microscope',
  'Home Care': 'home-heart',
  Clinical: 'medical-bag',
  Education: 'school',
  Rehab: 'wheelchair-accessibility',
  Mining: 'pickaxe',
  Manufacturing: 'factory',
}

// Maps a live API product onto the shape the cards render.
const mapApiProduct = (p: MobileProduct): Product => ({
  id: p.sku,
  name: p.productName,
  sku: p.sku,
  category: p.category,
  price: p.price,
  unit: p.isLowStock ? 'Low stock' : `${p.stockQty} in stock`,
  icon: CATEGORY_ICONS[p.category] ?? 'package-variant-closed',
  // The API returns a URL; React Native needs it wrapped as an image source.
  ...(p.imageUrl ? { image: { uri: p.imageUrl } as ImageSourcePropType } : null),
})

/* -------------------------------- Screen -------------------------------- */

const MedicalCatalogScreen = () => {
  const insets = useSafeAreaInsets()
  const router = useRouter()
  const [query, setQuery] = useState('')
  const [scannerOpen, setScannerOpen] = useState(false)
  const [activeFilter, setActiveFilter] = useState<Filter>('All')
  const [products, setProducts] = useState<Product[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [cartCount, setCartCount] = useState(() => getCartLines().reduce((sum, line) => sum + line.quantity, 0))

  useEffect(() => subscribeCart(() => setCartCount(getCartLines().reduce((sum, line) => sum + line.quantity, 0))), [])

  const loadProducts = useCallback(async (showSkeleton = false) => {
    if (showSkeleton) setLoading(true)
    setRefreshing(true)
    try {
      const live = await fetchMobileCatalogue()
      setProducts(live.map(mapApiProduct))
      setError(null)
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Could not load products.')
    } finally {
      setLoading(false)
      setRefreshing(false)
    }
  }, [])

  useFocusEffect(useCallback(() => { void loadProducts() }, [loadProducts]))
  useEffect(() => subscribeGlobalRefresh(() => { void loadProducts() }), [loadProducts])

  const chips = useMemo(() => {
    const live = Array.from(new Set(products.map((p) => p.category))).sort()
    return live
  }, [products])

  const filteredProducts = useMemo(() => {
    const term = query.trim().toLowerCase()
    return products.filter((product) => {
      const matchesCategory = activeFilter === 'All' || product.category === activeFilter
      const matchesSearch =
        term === '' ||
        product.name.toLowerCase().includes(term) ||
        product.sku.toLowerCase().includes(term) ||
        product.category.toLowerCase().includes(term)
      return matchesCategory && matchesSearch
    })
  }, [products, query, activeFilter])

  const renderProduct = ({ item, index }: { item: Product; index: number }) => (
    <Animated.View entering={FadeInDown.delay(Math.min(index * 55, 440)).duration(300)} style={styles.card}>
      <View style={styles.card_top}>
        <View style={styles.image_box}>
          {item.image ? (
            <Image source={item.image} style={styles.product_image} />
          ) : (
            <MaterialCommunityIcons name={item.icon} size={48} color="#37474f" />
          )}
        </View>

        <View style={styles.card_info}>
          <Text style={styles.product_name} numberOfLines={2}>
            {item.name}
          </Text>
          <Text style={styles.product_sku}>{item.sku}</Text>

          <View style={styles.meta_row}>
            <MaterialCommunityIcons name="warehouse" size={16} color={CRMTheme.primary} />
            <Text style={styles.rating}>{item.unit}</Text>
            <View style={styles.tag}>
              <Text style={styles.tag_text}>{item.category}</Text>
            </View>
          </View>
        </View>
      </View>

      <View style={styles.divider} />

      <View style={styles.card_bottom}>
        <View>
          <Text style={styles.price}>{formatPrice(item.price)}</Text>
          <Text style={styles.unit}>{item.unit}</Text>
        </View>

        <AnimatedButton shimmer onPress={() => addCartItem({ sku: item.sku, productName: item.name, price: item.price })}>
          <LinearGradient
            colors={[CRMTheme.primaryDark, CRMTheme.primary]}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 0 }}
            style={styles.add_button}
          >
            <Text style={styles.add_button_text}>Add to cart</Text>
          </LinearGradient>
        </AnimatedButton>
      </View>
    </Animated.View>
  )

  return (
    <View style={styles.screen}>
      {/* Header */}
      <LinearGradient
        colors={[CRMTheme.primaryDark, CRMTheme.primary]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={[styles.header, { paddingTop: insets.top + 16 }]}
      >
        <View style={styles.header_top}>
          <View>
              <Text style={styles.title}>Equipment Catalogue</Text>
            <Text style={styles.subtitle}>
              {filteredProducts.length} {filteredProducts.length === 1 ? 'product' : 'products'}
            </Text>
          </View>

          <TouchableOpacity style={styles.cart_button} accessibilityRole="button" accessibilityLabel={`Open cart, ${cartCount} items`} onPress={() => router.push('/cartScreen')}>
            <MaterialCommunityIcons name="cart-outline" size={28} color="#ffffff" />
            {cartCount > 0 && <View style={styles.cart_badge}><Text style={styles.cart_badge_text}>{cartCount > 99 ? '99+' : cartCount}</Text></View>}
          </TouchableOpacity>
        </View>

        <View style={styles.search_bar}>
          <TextInput
            style={styles.search_input}
            value={query}
            onChangeText={setQuery}
            placeholder="Search products"
            placeholderTextColor="#9e9e9e"
            autoCapitalize="none"
            autoCorrect={false}
            returnKeyType="search"
          />
          {query.length > 0 ? (
            <TouchableOpacity onPress={() => setQuery('')}>
              <MaterialCommunityIcons name="close-circle" size={20} color="#757575" />
            </TouchableOpacity>
          ) : (
            <MaterialCommunityIcons name="magnify" size={22} color="#546e7a" />
          )}
          <TouchableOpacity accessibilityRole="button" accessibilityLabel="Scan barcode" onPress={() => setScannerOpen(true)}>
            <MaterialCommunityIcons name="barcode-scan" size={22} color={CRMTheme.primary} />
          </TouchableOpacity>
        </View>
      </LinearGradient>

      {/* Category chips */}
      <View style={styles.chips_wrapper}>
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.chips}
        >
          {['All', ...chips].map((category) => {
            const selected = category === activeFilter
            return (
              <TouchableOpacity
                key={category}
                style={[styles.chip, selected && styles.chip_selected]}
                onPress={() => setActiveFilter(category)}
              >
                <Text style={[styles.chip_text, selected && styles.chip_text_selected]}>
                  {category}
                </Text>
              </TouchableOpacity>
            )
          })}
        </ScrollView>
      </View>

      {/* Product list */}
      <FlatList
        data={filteredProducts}
        keyExtractor={(item) => item.sku}
        renderItem={renderProduct}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={refreshing} tintColor={CRMTheme.primary} onRefresh={() => { void loadProducts() }} />}
        showsVerticalScrollIndicator={false}
        keyboardShouldPersistTaps="handled"
        ListEmptyComponent={loading ? <View style={{ paddingTop: 18 }}><SkeletonCard /><SkeletonCard /><SkeletonCard /></View> : error ? (
          <View style={styles.empty}><Text style={styles.empty_text}>{error}</Text><TouchableOpacity onPress={() => { setError(null); void loadProducts(true) }}><Text style={styles.empty_text}>Tap to retry</Text></TouchableOpacity></View>
        ) : (
          <View style={styles.empty}>
            <MaterialCommunityIcons name="package-variant-closed" size={48} color="#9e9e9e" />
            <Text style={styles.empty_text}>No products found</Text>
          </View>
        )}
      />
      <BarcodeScannerModal
        visible={scannerOpen}
        title="Scan product barcode"
        onClose={() => setScannerOpen(false)}
        onScanned={setQuery}
      />
    </View>
  )
}

export default MedicalCatalogScreen

/* -------------------------------- Styles -------------------------------- */

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: CRMTheme.background,
  },

  // Header
  header: {
    paddingHorizontal: 20,
    paddingBottom: 24,
    borderBottomLeftRadius: 40,
    borderBottomRightRadius: 40,
  },
  header_top: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 16,
  },
  title: {
    fontSize: 32,
    color: '#ffffff',
    fontWeight: '900',
  },
  subtitle: {
    color: '#ffffff',
    fontWeight: 'bold',
    fontSize: 15,
    marginTop: 2,
  },
  cart_button: {
    padding: 4,
  },
  cart_badge: {
    position: 'absolute',
    top: -2,
    right: -4,
    minWidth: 20,
    height: 20,
    borderRadius: 10,
    backgroundColor: '#d32f2f',
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 4,
  },
  cart_badge_text: {
    color: '#ffffff',
    fontSize: 11,
    fontWeight: 'bold',
  },
  search_bar: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: CRMTheme.surface,
    borderRadius: 20,
    paddingHorizontal: 14,
    height: 42,
  },
  search_input: {
    flex: 1,
    fontSize: 15,
    color: CRMTheme.text,
    paddingVertical: 0,
  },

  // Chips
  chips_wrapper: {
    paddingTop: 16,
  },
  chips: {
    paddingHorizontal: 16,
    gap: 10,
  },
  chip: {
    backgroundColor: CRMTheme.surface,
    borderWidth: 1,
    borderColor: CRMTheme.border,
    borderRadius: 18,
    paddingVertical: 8,
    paddingHorizontal: 22,
  },
  chip_selected: {
    backgroundColor: CRMTheme.primaryDark,
    borderColor: CRMTheme.primaryDark,
  },
  chip_text: {
    fontSize: 12,
    fontWeight: 'bold',
    color: CRMTheme.text,
  },
  chip_text_selected: {
    color: '#ffffff',
  },

  // List
  list: {
    paddingHorizontal: 16,
    paddingTop: 16,
    paddingBottom: 24,
    gap: 16,
    flexGrow: 1,
  },
  card: {
    backgroundColor: CRMTheme.surface,
    borderRadius: 18,
    borderWidth: 1,
    borderColor: CRMTheme.border,
    overflow: 'hidden',
  },
  card_top: {
    flexDirection: 'row',
    padding: 14,
    gap: 14,
    alignItems: 'center',
  },
  image_box: {
    width: 84,
    height: 72,
    justifyContent: 'center',
    alignItems: 'center',
  },
  product_image: {
    width: '100%',
    height: '100%',
    resizeMode: 'contain',
  },
  card_info: {
    flex: 1,
    gap: 4,
  },
  product_name: {
    fontSize: 15,
    fontWeight: '900',
    color: CRMTheme.text,
  },
  product_sku: {
    fontSize: 11,
    color: CRMTheme.muted,
  },
  meta_row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    marginTop: 2,
    flexWrap: 'wrap',
  },
  rating: {
    fontSize: 11,
    fontWeight: 'bold',
    color: CRMTheme.text,
  },
  tag: {
    borderWidth: 1,
    borderColor: CRMTheme.border,
    borderRadius: 10,
    paddingVertical: 2,
    paddingHorizontal: 12,
  },
  tag_text: {
    fontSize: 10,
    fontWeight: '600',
    color: CRMTheme.text,
  },
  divider: {
    height: 1,
    backgroundColor: CRMTheme.border,
  },
  card_bottom: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  price: {
    fontSize: 14,
    fontWeight: '900',
    color: CRMTheme.text,
  },
  unit: {
    fontSize: 11,
    fontWeight: 'bold',
    color: CRMTheme.text,
  },
  add_button: {
    paddingVertical: 8,
    paddingHorizontal: 18,
    borderRadius: 12,
  },
  add_button_text: {
    color: '#ffffff',
    fontWeight: 'bold',
    fontSize: 12,
  },

  // Empty state
  empty: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 60,
    gap: 10,
  },
  empty_text: {
    fontSize: 16,
    fontWeight: '600',
    color: CRMTheme.muted,
  },
})



