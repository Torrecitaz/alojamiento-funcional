<template>
  <div class="home-page">
    <section class="hero container">
      <div class="hero-content">
        <h1>Encuentra tu lugar <span class="highlight">ideal</span></h1>
        <p>Descubre los mejores alojamientos diseñados para tu comodidad y descanso, con una estética moderna y tonos cálidos.</p>
        <div class="hero-actions">
          <router-link to="/propiedades" class="btn-primary">Explorar Propiedades</router-link>
        </div>
      </div>
      <div class="hero-image">
        <div class="abstract-shape shape-1"></div>
        <div class="abstract-shape shape-2"></div>
      </div>
    </section>

    <section class="features container">
      <h2>¿Por qué elegirnos?</h2>
      <div class="features-grid">
        <div class="card feature-card">
          <div class="icon-wrapper">
            <i class="lucide-home"></i>
          </div>
          <h3>Alojamientos Premium</h3>
          <p>Seleccionamos cuidadosamente cada propiedad para garantizar tu confort.</p>
        </div>
        <div class="card feature-card">
          <div class="icon-wrapper">
            <i class="lucide-shield"></i>
          </div>
          <h3>Reservas Seguras</h3>
          <p>Tu información y pagos están protegidos con los más altos estándares.</p>
        </div>
        <div class="card feature-card">
          <div class="icon-wrapper">
            <i class="lucide-heart"></i>
          </div>
          <h3>Atención 24/7</h3>
          <p>Estamos aquí para ayudarte en cualquier momento durante tu estadía.</p>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.hero {
  display: flex;
  align-items: center;
  min-height: 70vh;
  gap: 4rem;
}

.hero-content {
  flex: 1;
}

.hero h1 {
  font-size: 4rem;
  line-height: 1.1;
  margin-bottom: 1.5rem;
  color: var(--color-text-primary);
}

.highlight {
  color: var(--color-primary-dark);
}

.hero p {
  font-size: 1.25rem;
  color: var(--color-text-secondary);
  margin-bottom: 2rem;
  max-width: 500px;
}

.hero-image {
  flex: 1;
  position: relative;
  height: 500px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.abstract-shape {
  position: absolute;
  border-radius: 50%;
  filter: blur(40px);
  z-index: -1;
  opacity: 0.6;
}

.shape-1 {
  width: 300px;
  height: 300px;
  background-color: var(--color-primary);
  top: 10%;
  left: 20%;
  animation: float 6s ease-in-out infinite;
}

.shape-2 {
  width: 250px;
  height: 250px;
  background-color: var(--color-secondary);
  bottom: 20%;
  right: 10%;
  animation: float 8s ease-in-out infinite reverse;
}

@keyframes float {
  0% { transform: translateY(0px) rotate(0deg); }
  50% { transform: translateY(-20px) rotate(10deg); }
  100% { transform: translateY(0px) rotate(0deg); }
}

.features {
  text-align: center;
  margin-top: 4rem;
}

.features h2 {
  font-size: 2.5rem;
  margin-bottom: 3rem;
  color: var(--color-primary-dark);
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
}

.feature-card {
  padding: 2.5rem 1.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.icon-wrapper {
  width: 64px;
  height: 64px;
  background-color: var(--color-secondary);
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-bottom: 1.5rem;
  color: var(--color-primary-dark);
  font-size: 1.5rem;
}

.feature-card h3 {
  font-size: 1.25rem;
  margin-bottom: 1rem;
}

@media (max-width: 768px) {
  .hero {
    flex-direction: column;
    text-align: center;
    min-height: auto;
    padding-top: 2rem;
  }
  
  .hero p {
    margin: 0 auto 2rem;
  }
  
  .hero h1 {
    font-size: 2.5rem;
  }
}
</style>
