const { withGradleProperties } = require('expo/config-plugins')

const gradleLimits = {
  'org.gradle.parallel': 'false',
  'org.gradle.workers.max': '2',
}

module.exports = function withAndroidGradleLimits(config) {
  return withGradleProperties(config, (config) => {
    for (const [key, value] of Object.entries(gradleLimits)) {
      const property = config.modResults.find(
        (item) => item.type === 'property' && item.key === key,
      )

      if (property) {
        property.value = value
      } else {
        config.modResults.push({ type: 'property', key, value })
      }
    }

    return config
  })
}
