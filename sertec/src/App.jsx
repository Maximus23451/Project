import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import PieChart from './demoPrototypes/pieChart.jsx'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <PieChart></PieChart>
    </>
  )
}

export default App
