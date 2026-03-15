import React, { useState, useEffect, useRef } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts';

import "./demoStyles/demo.css"

function PieChartDemo(){


  const pieceRef = useRef(null)



  
  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042'];

  const INITIAL_DATA = [
    { name: 'Group A', value: 400 },
    { name: 'Group B', value: 150 },
    { name: 'Group C', value: 500 },
    { name: 'Group D', value: 200 },
  ];




  useEffect(()=>{

    let sum=0
    for(let data of INITIAL_DATA){
      sum+=data.value
    }

    let currentDegree=360/sum


    let pieces=[]
    for(let item of INITIAL_DATA){
      pieces.push(item.value*currentDegree)
    }

    console.log(pieces + "pieces")

    const cum = [];
    let acc = 0;
    for (let p of pieces) {
      acc += p;
      cum.push(acc);
    }

    if (pieceRef.current) {
      const segments = pieces.map((_, i) => {
        const start = i === 0 ? 0 : cum[i - 1];
        const end = cum[i];
        const color = COLORS[i] || 'gray';

        return `${color} ${start}deg ${end}deg`;
        })


        let names=document.querySelector(".names")
        let colorDiv=document.createElement("div")
        INITIAL_DATA.forEach(item=>{
          console.log(item)
          colorDiv.innerHTML=`<div> <div style='background-color: ${COLORS[0]}; width: 10px; height: 10px; '></div> ${item.name}</div>`

          names.appendChild(colorDiv)

        });
      pieceRef.current.style.background = `conic-gradient(${segments.join(', ')})`;
    }

  },[])


  return(

    <>
      <div className='chart'>
        <div className='piece' ref={pieceRef}>
        </div>

      </div>


      <div className='names'>

      </div>
    
    
    </>



  )



}



export default PieChartDemo;