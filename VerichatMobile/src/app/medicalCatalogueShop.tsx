import { MaterialCommunityIcons } from '@expo/vector-icons'; //insert animated icons in expo.icon
import { LinearGradient } from 'expo-linear-gradient';
import { ComponentProps, useMemo, useState } from 'react';
import {
  FlatList,
  Image,
  ImageSourcePropType,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

/* ----------------------------- Types & data ----------------------------- */

type IconName = ComponentProps<typeof MaterialCommunityIcons>['name']

type Category = 'PPE' | 'Infant Care' | 'Hospital' | 'Lab' | 'Home Care'

type Product = {
  id: string
  name: string
  sku: string
  category: Category
  price: number
  unit: string // "each", "per pack", ...
  rating: number
  reviews: number
  icon: IconName
  // Optional: add a real product photo, e.g. require('../../assets/images/products/stethoscope.png')
  image?: ImageSourcePropType
}

const CURRENCY = '$'

const CATEGORIES = ['All', 'PPE', 'Infant Care', 'Hospital', 'Lab', 'Home Care'] as const
type Filter = (typeof CATEGORIES)[number]

// Static data. Replace with a backend call when the catalogue API is ready.
const PRODUCTS: Product[] = [
  {
    id: '1',
    name: 'Stethoscope',
    sku: 'ACC-STH-001',
    category: 'Hospital',
    price: 35,
    unit: 'per pack',
    rating: 4.8,
    reviews: 192,
    icon: 'stethoscope',
  },
  {
    id: '2',
    name: 'Surgical Face Masks (Box 100)',
    sku: 'PPE-MSK-100',
    category: 'PPE',
    price: 5,
    unit: 'per pack',
    rating: 4.7,
    reviews: 125,
    icon: 'face-mask',
  },
  {
    id: '3',
    name: 'Wheelchair',
    sku: 'HC-WCH-210',
    category: 'Home Care',
    price: 80,
    unit: 'each',
    rating: 4.6,
    reviews: 65,
    icon: 'wheelchair-accessibility',
  },
  {
    id: '4',
    name: 'Portable Ultrasound Scanner',
    sku: 'HSP-ULS-450',
    category: 'Hospital',
    price: 18500,
    unit: 'each',
    rating: 4.4,
    reviews: 84,
    icon: 'monitor-dashboard',
  },
  {
    id: '5',
    name: 'Digital Thermometer',
    sku: 'HC-THM-014',
    category: 'Home Care',
    price: 12.5,
    unit: 'each',
    rating: 4.5,
    reviews: 210,
    icon: 'thermometer',
  },
  {
    id: '6',
    name: 'Nitrile Examination Gloves (Box 100)',
    sku: 'PPE-GLV-100',
    category: 'PPE',
    price: 9,
    unit: 'per pack',
    rating: 4.8,
    reviews: 340,
    icon: 'hand-back-left',
  },
  {
    id: '7',
    name: 'Infant Nasal Aspirator',
    sku: 'INF-NSA-007',
    category: 'Infant Care',
    price: 7.5,
    unit: 'each',
    rating: 4.3,
    reviews: 58,
    icon: 'baby-face-outline',
  },
  {
    id: '8',
    name: 'Infant Weighing Scale',
    sku: 'INF-SCL-021',
    category: 'Infant Care',
    price: 45,
    unit: 'each',
    rating: 4.6,
    reviews: 77,
    icon: 'scale-bathroom',
  },
  {
    id: '9',
    name: 'Microscope Slides (Pack 50)',
    sku: 'LAB-SLD-050',
    category: 'Lab',
    price: 6,
    unit: 'per pack',
    rating: 4.4,
    reviews: 96,
    icon: 'microscope',
  },
  {
    id: '10',
    name: 'Test Tubes (Pack 100)',
    sku: 'LAB-TTB-100',
    category: 'Lab',
    price: 11,
    unit: 'per pack',
    rating: 4.5,
    reviews: 143,
    icon: 'test-tube',
  },
]

const SERIF = Platform.select({ ios: 'Georgia', android: 'serif', default: 'serif' })

const formatPrice = (value: number) =>
  `${CURRENCY}${value.toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`

/* -------------------------------- Screen -------------------------------- */

const MedicalCatalogScreen = () => {
  const insets = useSafeAreaInsets()
  const [query, setQuery] = useState('')
  const [activeFilter, setActiveFilter] = useState<Filter>('All')
  const [cart, setCart] = useState<Record<string, number>>({})

  const cartCount = Object.values(cart).reduce((sum, qty) => sum + qty, 0)

  const filteredProducts = useMemo(() => {
    const term = query.trim().toLowerCase()
    return PRODUCTS.filter((product) => {
      const matchesCategory = activeFilter === 'All' || product.category === activeFilter
      const matchesSearch =
        term === '' ||
        product.name.toLowerCase().includes(term) ||
        product.sku.toLowerCase().includes(term) ||
        product.category.toLowerCase().includes(term)
      return matchesCategory && matchesSearch
    })
  }, [query, activeFilter])

  const addToCart = (id: string) => setCart((prev) => ({ ...prev, [id]: (prev[id] ?? 0) + 1 }))

  const renderProduct = ({ item }: { item: Product }) => (
    <View style={styles.card}>
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
            <MaterialCommunityIcons name="star" size={16} color="#f9a825" />
            <Text style={styles.rating}>
              {item.rating.toFixed(1)} ({item.reviews})
            </Text>
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

        <TouchableOpacity activeOpacity={0.85} onPress={() => addToCart(item.id)}>
          <LinearGradient
            colors={['#004e7a', '#2f7be0']}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 0 }}
            style={styles.add_button}
          >
            <Text style={styles.add_button_text}>+ Add to cart</Text>
          </LinearGradient>
        </TouchableOpacity>
      </View>
    </View>
  )

  return (
    <View style={styles.screen}>
      {/* Header */}
      <LinearGradient
        colors={['#1f5f8b', '#3d7bd9']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={[styles.header, { paddingTop: insets.top + 16 }]}
      >
        <View style={styles.header_top}>
          <View>
            <Text style={styles.title}>Medical Catalog</Text>
            <Text style={styles.subtitle}>
              {filteredProducts.length} {filteredProducts.length === 1 ? 'product' : 'products'}
            </Text>
          </View>

          <TouchableOpacity
            style={styles.cart_button}
            onPress={() => {
              // TODO: navigate to the cart screen
            }}
          >
            <MaterialCommunityIcons name="cart-outline" size={30} color="#0a2f55" />
            {cartCount > 0 && (
              <View style={styles.cart_badge}>
                <Text style={styles.cart_badge_text}>{cartCount > 99 ? '99+' : cartCount}</Text>
              </View>
            )}
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
        </View>
      </LinearGradient>

      {/* Category chips */}
      <View style={styles.chips_wrapper}>
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.chips}
        >
          {CATEGORIES.map((category) => {
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
        keyExtractor={(item) => item.id}
        renderItem={renderProduct}
        contentContainerStyle={styles.list}
        showsVerticalScrollIndicator={false}
        keyboardShouldPersistTaps="handled"
        ListEmptyComponent={
          <View style={styles.empty}>
            <MaterialCommunityIcons name="package-variant-closed" size={48} color="#9e9e9e" />
            <Text style={styles.empty_text}>No products found</Text>
          </View>
        }
      />
    </View>
  )
}

export default MedicalCatalogScreen

/* -------------------------------- Styles -------------------------------- */

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: '#efefef',
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
    fontStyle: 'italic',
    fontWeight: '900',
    fontFamily: SERIF,
    textShadowColor: 'rgba(255,255,255,0.45)',
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 8,
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
    backgroundColor: '#ffffff',
    borderRadius: 20,
    paddingHorizontal: 14,
    height: 42,
  },
  search_input: {
    flex: 1,
    fontSize: 15,
    color: '#000000',
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
    backgroundColor: '#ffffff',
    borderWidth: 1,
    borderColor: '#9e9e9e',
    borderRadius: 18,
    paddingVertical: 8,
    paddingHorizontal: 22,
  },
  chip_selected: {
    backgroundColor: '#001a3d',
    borderColor: '#001a3d',
  },
  chip_text: {
    fontSize: 12,
    fontWeight: 'bold',
    color: '#000000',
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
    backgroundColor: '#ffffff',
    borderRadius: 18,
    borderWidth: 1,
    borderColor: '#d0d0d0',
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
    color: '#000000',
  },
  product_sku: {
    fontSize: 11,
    color: '#9e9e9e',
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
    color: '#000000',
  },
  tag: {
    borderWidth: 1,
    borderColor: '#9e9e9e',
    borderRadius: 10,
    paddingVertical: 2,
    paddingHorizontal: 12,
  },
  tag_text: {
    fontSize: 10,
    fontWeight: '600',
    color: '#000000',
  },
  divider: {
    height: 1,
    backgroundColor: '#d0d0d0',
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
    color: '#000000',
  },
  unit: {
    fontSize: 11,
    fontWeight: 'bold',
    color: '#000000',
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
    color: '#757575',
  },
})